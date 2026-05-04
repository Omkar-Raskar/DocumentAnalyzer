namespace DocumentAnalyzer.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbedding(string text);
    }
}
