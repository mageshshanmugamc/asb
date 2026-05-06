namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Requests;
    using ASB.Admin.v1.Response;
    using ASB.Agent.v1.Dtos;
    using ASB.Agent.v1.Interfaces;
    using ASB.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    [ApiController]
    [Route("api/v1/[controller]")]
    [AsbAuthorize]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;
        private readonly IRagService _ragService;

        public AgentController(IAgentService agentService, IRagService ragService)
        {
            _agentService = agentService;
            _ragService = ragService;
        }

        /// <summary>
        /// Send a message to the AI agent. The agent uses RAG + tools to respond.
        /// </summary>
        [HttpPost("chat")]
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
        [AsbAuthorize(Policies.FullAccess)]
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
