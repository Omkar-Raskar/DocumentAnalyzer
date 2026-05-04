using DocumentAnalyzer.Services;
using System.Net.Http.Json;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public OllamaEmbeddingService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<float[]> GetEmbedding(string text)
    {
        var model = _config["Ollama:Model"];

        var request = new
        {
            model = model,
            prompt = text
        };

        var response = await _httpClient.PostAsJsonAsync(
            "http://localhost:11434/api/embeddings",
            request);

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

        return result.embedding;
    }
}

public class OllamaResponse
{
    public float[] embedding { get; set; }
}