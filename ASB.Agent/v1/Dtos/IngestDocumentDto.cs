namespace ASB.Agent.v1.Dtos;

public class IngestDocumentDto
{
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int ChunkSize { get; set; } = 500;
    public int ChunkOverlap { get; set; } = 50;
}
