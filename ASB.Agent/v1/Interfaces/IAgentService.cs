using ASB.Agent.v1.Dtos;

namespace ASB.Agent.v1.Interfaces;

/// <summary>
/// The AI agent orchestrator - manages the LLM conversation loop, 
/// tool calling, and RAG-augmented responses.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Sends a user message to the agent, which may use tools and RAG to answer.
    /// </summary>
    Task<ChatResponseDto> ChatAsync(ChatRequestDto request, string userId);
}
