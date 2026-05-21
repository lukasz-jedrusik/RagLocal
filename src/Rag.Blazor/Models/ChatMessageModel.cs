namespace Rag.Blazor.Models
{
    public class ChatMessageModel
    {
        public string Text { get; set; } = string.Empty;
        public bool IsUser { get; set; }

        public DateTime Timestamp { get; set; }
            = DateTime.UtcNow;
    }
}