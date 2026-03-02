namespace Rag.Services.Backend.Domain.Models
{
    public class SearchResult
    {
        public string Text { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public float Score { get; set; }
    }
}