using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DocumentAnalyzer.Services;

public class JinaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public JinaEmbeddingService(
        HttpClient httpClient,
        IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<float[]> GetEmbedding(string text)
    {
        var apiKey = _config["Jina:ApiKey"];

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);

        var body = new
        {
            model = "jina-embeddings-v3",
            input = new[] { text }
        };

        var response =
            await _httpClient.PostAsync(
                "https://api.jina.ai/v1/embeddings",
                new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"));

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content.ReadAsStringAsync();

        using var doc =
            JsonDocument.Parse(json);

        var embedding =
            doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding");

        return embedding
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();
    }
}