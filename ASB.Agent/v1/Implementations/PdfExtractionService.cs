using System.Text;
using ASB.Agent.v1.Interfaces;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ASB.Agent.v1.Implementations;



/// <summary>
/// Implementation of PDF text extraction using UglyToad.PdfPig.
/// </summary>
public class PdfExtractionService : IPdfExtractionService
{
    private readonly ILogger<PdfExtractionService> _logger;

    public PdfExtractionService(ILogger<PdfExtractionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extracts text content from a PDF file stream.
    /// </summary>
    public async Task<string> ExtractTextFromPdfAsync(Stream fileStream)
    {
        // Convert stream to byte array for PdfDocument
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var textBuilder = new StringBuilder();

        try
        {
            using (var pdfDocument = PdfDocument.Open(memoryStream.ToArray()))
            {
                _logger.LogInformation("Processing PDF with {PageCount} pages", pdfDocument.NumberOfPages);

                for (int pageIndex = 0; pageIndex < pdfDocument.NumberOfPages; pageIndex++)
                {
                    try
                    {
                        var page = pdfDocument.GetPage(pageIndex + 1);
                        var pageText = ExtractTextFromPage(page);

                        if (!string.IsNullOrWhiteSpace(pageText))
                        {
                            textBuilder.AppendLine($"--- Page {pageIndex + 1} ---");
                            textBuilder.AppendLine(pageText);
                            textBuilder.AppendLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error extracting text from page {PageNumber}", pageIndex + 1);
                        // Continue with next page even if one fails
                    }
                }
            }

            var extractedText = textBuilder.ToString().Trim();

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                _logger.LogWarning("No text content could be extracted from the PDF");
                throw new InvalidOperationException("PDF file appears to be empty or contains no extractable text.");
            }

            _logger.LogInformation("Successfully extracted {CharCount} characters from PDF", extractedText.Length);
            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PDF file");
            throw new InvalidOperationException("Failed to process PDF file. Please ensure it is a valid PDF document.", ex);
        }
    }

    /// <summary>
    /// Extracts text from a single PDF page.
    /// </summary>
    private static string ExtractTextFromPage(Page page)
    {
        var textBuilder = new StringBuilder();

        try
        {
            foreach (var textBlock in page.GetWords())
            {
                textBuilder.Append(textBlock.Text);
                textBuilder.Append(" ");
            }
        }
        catch
        {
            // If word extraction fails, try letter extraction
            foreach (var letter in page.Letters)
            {
                textBuilder.Append(letter.Value);
            }
        }

        return textBuilder.ToString();
    }
}
