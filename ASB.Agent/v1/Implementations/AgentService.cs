using System.Collections.Concurrent;
using System.Text;
using ASB.Agent.v1.Dtos;
using ASB.Agent.v1.Interfaces;
using ASB.Agent.v1.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace ASB.Agent.v1.Implementations;

/// <summary>
/// Agent orchestrator: manages the Semantic Kernel agent loop with tool calling,
/// RAG context injection, and conversation history.
/// </summary>
public class AgentService : IAgentService
{
    private readonly Kernel _kernel;
    private readonly IRagService _ragService;
    private readonly ILogger<AgentService> _logger;

    // In-memory conversation history (per conversation ID)
    // In production, replace with a persistent store (Redis, DB, etc.)
    private static readonly ConcurrentDictionary<string, ChatHistory> _conversations = new();

    private const string SystemPrompt = """
        You are an intelligent assistant for the ASB (Admin Service Backend) platform.
        You help users manage roles, policies, users, user groups, and menus.
        
        When answering questions:
        1. If relevant context documents are provided, use them to ground your answer.
        2. If you have access to tools, use them to fetch real-time data when needed.
        3. Be concise and accurate. Cite sources when using retrieved documents.
        4. If you don't know the answer, say so honestly.
        """;

    public AgentService(
        Kernel kernel,
        IRagService ragService,
        ILogger<AgentService> logger)
    {
        _kernel = kernel;
        _ragService = ragService;
        _logger = logger;
    }

    public async Task<ChatResponseDto> ChatAsync(ChatRequestDto request, string userId)
    {
        var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();
        var history = _conversations.GetOrAdd(conversationId, _ => new ChatHistory(SystemPrompt));

        // Step 1: Retrieve relevant context via RAG
        var relevantDocs = await _ragService.SearchAsync(request.Message);
        var sourceDocuments = new List<string>();

        // Step 2: Augment the user message with retrieved context
        var augmentedMessage = new StringBuilder();

        if (relevantDocs.Count > 0)
        {
            augmentedMessage.AppendLine("### Relevant Context (from knowledge base):");
            for (int i = 0; i < relevantDocs.Count; i++)
            {
                augmentedMessage.AppendLine($"[{i + 1}] {relevantDocs[i]}");
                sourceDocuments.Add($"Document chunk {i + 1}");
            }
            augmentedMessage.AppendLine();
            augmentedMessage.AppendLine("### User Question:");
        }

        augmentedMessage.AppendLine(request.Message);

        history.AddUserMessage(augmentedMessage.ToString());

        _logger.LogInformation(
            "Agent chat - ConversationId: {ConversationId}, UserId: {UserId}, RAG docs: {DocCount}",
            conversationId, userId, relevantDocs.Count);

        // Step 3: Call the LLM with tool-calling enabled via Semantic Kernel
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();

        var executionSettings = new OllamaPromptExecutionSettings
        {
            Temperature = 0.3f,
            // Enable auto tool invocation so the agent can call registered plugins
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var toolsUsed = new List<string>();

        // The agent loop: Semantic Kernel automatically handles tool calls
        // and re-invokes the LLM with tool results until it produces a final answer
        var result = await chatService.GetChatMessageContentAsync(
            history,
            executionSettings,
            _kernel);

        // Track any tools that were invoked
        if (result.Metadata?.TryGetValue("ToolCalls", out var tools) == true)
        {
            toolsUsed.Add(tools?.ToString() ?? "tool_call");
        }

        var assistantMessage = result.Content ?? "I'm sorry, I couldn't generate a response.";
        history.AddAssistantMessage(assistantMessage);

        return new ChatResponseDto
        {
            Response = assistantMessage,
            ConversationId = conversationId,
            SourceDocuments = sourceDocuments,
            ToolsUsed = toolsUsed
        };
    }
}
