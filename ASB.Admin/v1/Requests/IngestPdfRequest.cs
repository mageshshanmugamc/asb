namespace ASB.Admin.v1.Requests;

/// <summary>
/// Request model for ingesting PDF documents into the knowledge base.
/// </summary>
public class IngestPdfRequest
{
    /// <summary>
    /// The PDF file to upload (multipart form data).
    /// </summary>
    public IFormFile? PdfFile { get; set; }

    /// <summary>
    /// Custom source/document name. If not provided, the filename will be used.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Text chunk size for RAG processing (default: 500 characters).
    /// </summary>
    public int ChunkSize { get; set; } = 500;

    /// <summary>
    /// Overlap between chunks (default: 50 characters).
    /// </summary>
    public int ChunkOverlap { get; set; } = 50;
}
