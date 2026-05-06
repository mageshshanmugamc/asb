namespace ASB.Admin.v1.Requests;

public class IngestDocumentRequest
{
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int ChunkSize { get; set; } = 500;
    public int ChunkOverlap { get; set; } = 50;
}
