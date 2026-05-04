public class LLMServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;

    public LLMServiceFactory(IServiceProvider serviceProvider, IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    public ILLMService GetService()
    {
        var provider = _config["LLM:Provider"];

        return provider switch
        {
            "Ollama" => _serviceProvider.GetRequiredService<OllamaLLMService>(),
            "OpenRouter" => _serviceProvider.GetRequiredService<OpenRouterLLMService>(),
            _ => throw new Exception("Invalid LLM Provider")
        };
    }
}