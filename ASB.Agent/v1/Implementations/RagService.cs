using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ASB.Agent.v1.Dtos;
using ASB.Agent.v1.Interfaces;
using ASB.Agent.v1.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ASB.Agent.v1.Implementations;

/// <summary>
/// RAG service: handles text chunking, embedding generation via Ollama,
/// and vector storage/search via Qdrant.
/// </summary>
public class RagService : IRagService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _ollamaSettings;
    private readonly QdrantSettings _qdrantSettings;
    private readonly ILogger<RagService> _logger;

    public RagService(
        HttpClient httpClient,
        IOptions<OllamaSettings> ollamaSettings,
        IOptions<QdrantSettings> qdrantSettings,
        ILogger<RagService> logger)
    {
        _httpClient = httpClient;
        _ollamaSettings = ollamaSettings.Value;
        _qdrantSettings = qdrantSettings.Value;
        _logger = logger;
    }

    public async Task<int> IngestDocumentAsync(IngestDocumentDto document)
    {
        var chunks = ChunkText(document.Content, document.ChunkSize, document.ChunkOverlap);
        _logger.LogInformation("Chunked document '{Source}' into {Count} chunks", document.Source, chunks.Count);

        await EnsureCollectionExistsAsync();

        var points = new List<object>();

        for (int i = 0; i < chunks.Count; i++)
        {
            var embedding = await GenerateEmbeddingAsync(chunks[i]);
            var id = Guid.NewGuid().ToString();

            points.Add(new
            {
                id,
                vector = embedding,
                payload = new
                {
                    content = chunks[i],
                    source = document.Source,
                    chunk_index = i
                }
            });
        }

        // Upsert points to Qdrant
        var upsertUrl = $"{_qdrantSettings.Endpoint}/collections/{_qdrantSettings.CollectionName}/points";
        var upsertBody = new { points };
        var response = await _httpClient.PutAsJsonAsync(upsertUrl, upsertBody);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Ingested {Count} chunks from '{Source}' into Qdrant", chunks.Count, document.Source);
        return chunks.Count;
    }

    public async Task<List<string>> SearchAsync(string query, int topK = 3)
    {
        // Check if the collection exists before searching
        var checkUrl = $"{_qdrantSettings.Endpoint}/collections/{_qdrantSettings.CollectionName}";
        try
        {
            var checkResponse = await _httpClient.GetAsync(checkUrl);
            if (!checkResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Qdrant collection '{Collection}' does not exist yet. No documents to search.", _qdrantSettings.CollectionName);
                return [];
            }
        }
        catch (HttpRequestException)
        {
            _logger.LogWarning("Could not reach Qdrant at {Endpoint}. Skipping RAG search.", _qdrantSettings.Endpoint);
            return [];
        }

        var queryEmbedding = await GenerateEmbeddingAsync(query);

        var searchUrl = $"{_qdrantSettings.Endpoint}/collections/{_qdrantSettings.CollectionName}/points/search";
        var searchBody = new
        {
            vector = queryEmbedding,
            limit = topK,
            with_payload = true
        };

        var response = await _httpClient.PostAsJsonAsync(searchUrl, searchBody);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Qdrant search failed with status {Status}", response.StatusCode);
            return [];
        }

        var result = await response.Content.ReadFromJsonAsync<QdrantSearchResponse>();
        if (result?.Result == null) return [];

        return result.Result
            .Select(r => r.Payload != null && r.Payload.TryGetValue("content", out var content) ? content.ToString() : "")
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();
    }

    /// <summary>
    /// Simple text chunking with overlap - splits text into fixed-size chunks.
    /// </summary>
    private static List<string> ChunkText(string text, int chunkSize, int overlap)
    {
        var chunks = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return chunks;

        // Split by sentences for more natural boundaries
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new StringBuilder();
        var wordCount = 0;
        var overlapBuffer = new Queue<string>();

        foreach (var word in words)
        {
            currentChunk.Append(word).Append(' ');
            wordCount++;

            if (wordCount >= chunkSize)
            {
                chunks.Add(currentChunk.ToString().Trim());

                // Keep overlap words for next chunk
                currentChunk.Clear();
                var chunkWords = chunks[^1].Split(' ');
                var overlapStart = Math.Max(0, chunkWords.Length - overlap);
                for (int i = overlapStart; i < chunkWords.Length; i++)
                {
                    currentChunk.Append(chunkWords[i]).Append(' ');
                }

                wordCount = overlap;
            }
        }

        if (currentChunk.Length > 0)
            chunks.Add(currentChunk.ToString().Trim());

        return chunks;
    }

    /// <summary>
    /// Generates an embedding vector using Ollama's embedding API.
    /// </summary>
    private async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var url = $"{_ollamaSettings.Endpoint}/api/embed";
        var body = new { model = _ollamaSettings.EmbeddingModel, input = text };

        var response = await _httpClient.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>();
        return result?.Embeddings?.FirstOrDefault() ?? [];
    }

    /// <summary>
    /// Ensures the Qdrant collection exists, creating it if necessary.
    /// </summary>
    private async Task EnsureCollectionExistsAsync()
    {
        var checkUrl = $"{_qdrantSettings.Endpoint}/collections/{_qdrantSettings.CollectionName}";

        try
        {
            var response = await _httpClient.GetAsync(checkUrl);
            if (response.IsSuccessStatusCode) return;
        }
        catch
        {
            // Collection doesn't exist, create it
        }

        var createUrl = $"{_qdrantSettings.Endpoint}/collections/{_qdrantSettings.CollectionName}";
        var createBody = new
        {
            vectors = new
            {
                size = _qdrantSettings.VectorSize,
                distance = "Cosine"
            }
        };

        var createResponse = await _httpClient.PutAsJsonAsync(createUrl, createBody);
        createResponse.EnsureSuccessStatusCode();
        _logger.LogInformation("Created Qdrant collection '{Collection}'", _qdrantSettings.CollectionName);
    }

    // Internal response models for Ollama/Qdrant APIs
    private record OllamaEmbedResponse(float[][]? Embeddings);

    private record QdrantSearchResponse(List<QdrantSearchResult>? Result);

    private record QdrantSearchResult(
        string Id,
        float Score,
        Dictionary<string, JsonElement>? Payload);
}
