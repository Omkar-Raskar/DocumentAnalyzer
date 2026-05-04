public interface ILLMService
{
    Task<string> GenerateResponse(string prompt);
}