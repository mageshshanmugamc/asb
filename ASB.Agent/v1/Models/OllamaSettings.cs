namespace ASB.Agent.v1.Models;

/// <summary>
/// Configuration for Ollama LLM and embedding endpoints.
/// </summary>
public class OllamaSettings
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string ChatModel { get; set; } = "phi3:mini";
    public string EmbeddingModel { get; set; } = "nomic-embed-text";
}
