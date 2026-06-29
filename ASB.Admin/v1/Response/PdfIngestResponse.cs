namespace ASB.Admin.v1.Response;

/// <summary>
/// Response from PDF ingestion into the knowledge base.
/// </summary>
public class PdfIngestResponse
{
    /// <summary>
    /// Indicates whether the PDF was successfully processed and ingested.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Descriptive message about the ingestion result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Original PDF file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Source identifier used in the knowledge base (document name).
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Number of text chunks created from the PDF.
    /// </summary>
    public int ChunksCreated { get; set; }

    /// <summary>
    /// Total length of extracted text content (in characters).
    /// </summary>
    public int TextLength { get; set; }

    /// <summary>
    /// Size of the uploaded PDF file (in bytes).
    /// </summary>
    public long FileSize { get; set; }
}
