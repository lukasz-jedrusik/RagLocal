namespace Rag.Blazor.Models
{
    public class StreamChunk
    {
        public string Content { get; set; } = string.Empty;
        public Guid? ConversationId { get; set; }
        public bool IsConversationId { get; set; }
        public bool IsCompleted { get; set; }
    }
}
