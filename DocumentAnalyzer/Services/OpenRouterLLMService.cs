using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class OpenRouterLLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public OpenRouterLLMService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> GenerateResponse(string prompt)
    {
        var apiKey = _config["OpenRouter:ApiKey"];
        var model = _config["OpenRouter:Model"];

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            content);

        var responseString = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseString);

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }
}