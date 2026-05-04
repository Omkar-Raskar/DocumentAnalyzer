using System.Text;
using System.Text.Json;

namespace DocumentAnalyzer.Services
{

    public class EmbeddingService
    {
        private readonly HttpClient _httpClient;

        public EmbeddingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<float>> GetEmbedding(string text)
        {
            var request = new
            {
                model = "nomic-embed-text",
                prompt = text
            };

            var response = await _httpClient.PostAsync(
                "http://localhost:11434/api/embeddings",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            );

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var embeddingArray = doc.RootElement.GetProperty("embedding");

            return embeddingArray.EnumerateArray()
                .Select(x => x.GetSingle())
                .ToList();
        }
    }
}
