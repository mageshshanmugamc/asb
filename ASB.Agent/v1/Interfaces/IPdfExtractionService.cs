namespace ASB.Agent.v1.Interfaces;

/// <summary>
/// Service for extracting text content from PDF files.
/// </summary>
public interface IPdfExtractionService
{
    /// <summary>
    /// Extracts text content from a PDF file stream.
    /// </summary>
    /// <param name="fileStream">The PDF file stream to extract text from.</param>
    /// <returns>The extracted text content from the PDF.</returns>
    Task<string> ExtractTextFromPdfAsync(Stream fileStream);
}