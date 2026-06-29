namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Requests;
    using ASB.Admin.v1.Response;
    using ASB.Agent.v1.Dtos;
    using ASB.Agent.v1.Interfaces;
    using ASB.Authorization;
    using Microsoft.AspNetCore.Http.Timeouts;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    [ApiController]
    [Route("api/v1/[controller]")]
    // [AsbAuthorize]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;
        private readonly IRagService _ragService;
        private readonly IPdfExtractionService _pdfExtractionService;

        public AgentController(IAgentService agentService, IRagService ragService, IPdfExtractionService pdfExtractionService)
        {
            _agentService = agentService;
            _ragService = ragService;
            _pdfExtractionService = pdfExtractionService;
        }

        /// <summary>
        /// Send a message to the AI agent. The agent uses RAG + tools to respond.
        /// </summary>
        [HttpPost("chat")]
        [RequestTimeout(600000)] // 10 minutes for slow CPU inference
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

            var dto = new ChatRequestDto
            {
                Message = request.Message,
                ConversationId = request.ConversationId
            };

            var result = await _agentService.ChatAsync(dto, userId);

            return Ok(new ChatResponse
            {
                Response = result.Response,
                ConversationId = result.ConversationId,
                SourceDocuments = result.SourceDocuments,
                ToolsUsed = result.ToolsUsed
            });
        }

        /// <summary>
        /// Ingest a document into the vector database for RAG retrieval.
        /// </summary>
        [HttpPost("ingest")]
        // [AsbAuthorize(Policies.FullAccess)]
        public async Task<IActionResult> IngestDocument([FromBody] IngestDocumentRequest request)
        {
            var dto = new IngestDocumentDto
            {
                Content = request.Content,
                Source = request.Source,
                ChunkSize = request.ChunkSize,
                ChunkOverlap = request.ChunkOverlap
            };

            var chunksIngested = await _ragService.IngestDocumentAsync(dto);

            return Ok(new { chunksIngested, source = request.Source });
        }

        /// <summary>
        /// Ingest a PDF document into the knowledge base. 
        /// Upload PDF file via multipart/form-data. The text will be extracted and stored in the vector database.
        /// </summary>
        /// <param name="request">The PDF file upload request containing the PDF file and optional parameters.</param>
        /// <returns>Number of chunks created and metadata about the ingested document.</returns>
        [HttpPost("ingest/pdf")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(PdfIngestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // [AsbAuthorize(Policies.FullAccess)]
        public async Task<IActionResult> IngestPdfDocument([FromForm] IngestPdfRequest request)
        {
            if (request.PdfFile == null || request.PdfFile.Length == 0)
            {
                return BadRequest(new { error = "PDF file is required." });
            }

            try
            {
                // Extract text from PDF
                using (var fileStream = request.PdfFile.OpenReadStream())
                {
                    var extractedText = await _pdfExtractionService.ExtractTextFromPdfAsync(fileStream);

                    if (string.IsNullOrWhiteSpace(extractedText))
                    {
                        return BadRequest(new { error = "No text content could be extracted from the PDF file." });
                    }

                    // Determine source name
                    var sourceName = !string.IsNullOrWhiteSpace(request.Source)
                        ? request.Source
                        : Path.GetFileNameWithoutExtension(request.PdfFile.FileName);

                    // Ingest the extracted text into the knowledge base
                    var dto = new IngestDocumentDto
                    {
                        Content = extractedText,
                        Source = sourceName,
                        ChunkSize = request.ChunkSize,
                        ChunkOverlap = request.ChunkOverlap
                    };

                    var chunksIngested = await _ragService.IngestDocumentAsync(dto);

                    return Ok(new PdfIngestResponse
                    {
                        Success = true,
                        Message = $"PDF document successfully ingested into knowledge base.",
                        FileName = request.PdfFile.FileName,
                        Source = sourceName,
                        ChunksCreated = chunksIngested,
                        TextLength = extractedText.Length,
                        FileSize = request.PdfFile.Length
                    });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while processing the PDF file.", details = ex.Message });
            }
        }

        /// <summary>
        /// Search the knowledge base directly (useful for debugging RAG).
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] ChatRequest request)
        {
            var results = await _ragService.SearchAsync(request.Message);
            return Ok(new { query = request.Message, results });
        }
    }
}
