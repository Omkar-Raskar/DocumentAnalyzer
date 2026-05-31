namespace DocumentAnalyzer.Services;

public class EmbeddingServiceFactory
{
    private readonly IServiceProvider _provider;
    private readonly IConfiguration _config;

    public EmbeddingServiceFactory(
        IServiceProvider provider,
        IConfiguration config)
    {
        _provider = provider;
        _config = config;
    }

    public IEmbeddingService GetService()
    {
        var providerName =
            _config["Embedding:Provider"];

        return providerName switch
        {
            "Jina" =>
                _provider.GetRequiredService<JinaEmbeddingService>(),

            _ =>
                _provider.GetRequiredService<OllamaEmbeddingService>()
        };
    }
}