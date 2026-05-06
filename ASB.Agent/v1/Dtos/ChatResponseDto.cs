namespace ASB.Agent.v1.Dtos;

public class ChatResponseDto
{
    public string Response { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public List<string> SourceDocuments { get; set; } = [];
    public List<string> ToolsUsed { get; set; } = [];
}
