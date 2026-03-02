namespace Rag.Services.Backend.Application.Interfaces.Services
{
    public interface IOllamaService
    {
        Task<string> AskAsync(string prompt);
        Task<float[]> CreateAsync(string text);
    }
}