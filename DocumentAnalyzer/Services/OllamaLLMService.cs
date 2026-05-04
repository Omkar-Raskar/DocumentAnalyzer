using System.Net.Http.Json;

public class OllamaLLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public OllamaLLMService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> GenerateResponse(string prompt)
    {
        var model = _config["Ollama:Model"];

        var request = new
        {
            model = model,
            prompt = prompt,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
    "http://localhost:11434/api/generate",
    request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ollama Error: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();

        return result?.response ?? "No response from model";
    }
}

public class OllamaGenerateResponse
{
    public string response { get; set; }
}