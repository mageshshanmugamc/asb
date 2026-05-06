namespace ASB.Agent.v1.Models;

/// <summary>
/// Represents a document chunk stored in the vector database.
/// </summary>
public class DocumentChunk
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public ReadOnlyMemory<float> Embedding { get; set; }
}
