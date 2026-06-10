namespace Rag.Services.Backend.Domain.Models
{
    public class MessageSource
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int SourceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public double Score { get; set; }
        public Message Message { get; set; } = null!;
    }
}
