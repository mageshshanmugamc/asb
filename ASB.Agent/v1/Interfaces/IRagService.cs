using ASB.Agent.v1.Dtos;

namespace ASB.Agent.v1.Interfaces;

/// <summary>
/// Handles vector storage: embedding generation, document ingestion, and similarity search.
/// </summary>
public interface IRagService
{
    /// <summary>
    /// Chunks text, generates embeddings via Ollama, and stores them in Qdrant.
    /// </summary>
    Task<int> IngestDocumentAsync(IngestDocumentDto document);

    /// <summary>
    /// Retrieves the top-K most relevant chunks for a user query.
    /// </summary>
    Task<List<string>> SearchAsync(string query, int topK = 3);
}
