namespace ASB.Admin.v1.Response;

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public List<string> SourceDocuments { get; set; } = [];
    public List<string> ToolsUsed { get; set; } = [];
}
