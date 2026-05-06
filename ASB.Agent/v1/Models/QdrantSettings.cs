namespace ASB.Agent.v1.Models;

/// <summary>
/// Configuration for Qdrant vector database.
/// </summary>
public class QdrantSettings
{
    public string Endpoint { get; set; } = "http://localhost:6333";
    public string CollectionName { get; set; } = "asb_knowledge";
    public int VectorSize { get; set; } = 768; // nomic-embed-text dimension
}
