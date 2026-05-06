using ASB.Agent.v1.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ASB.Agent.v1.Plugins;

/// <summary>
/// Semantic Kernel plugin that exposes the RAG knowledge base search as a tool.
/// The LLM can decide to search for additional context during a conversation.
/// </summary>
public class KnowledgeBasePlugin
{
    private readonly IRagService _ragService;

    public KnowledgeBasePlugin(IRagService ragService)
    {
        _ragService = ragService;
    }

    [KernelFunction("search_knowledge_base")]
    [Description("Searches the knowledge base for relevant information about ASB platform documentation, policies, procedures, and guidelines. Use this when the user asks about how things work or needs reference material.")]
    public async Task<string> SearchKnowledgeBaseAsync(
        [Description("The search query to find relevant documents")] string query,
        [Description("Number of results to return (default 3)")] int topK = 3)
    {
        var results = await _ragService.SearchAsync(query, topK);

        if (results.Count == 0)
            return "No relevant documents found in the knowledge base.";

        return string.Join("\n\n---\n\n", results.Select((r, i) => $"[{i + 1}] {r}"));
    }
}
