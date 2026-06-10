namespace Rag.Services.Backend.Domain.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        /// <summary>
        /// &quot;user&quot; or &quot;assistant&quot;
        /// </summary>
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Conversation Conversation { get; set; } = null!;        public ICollection<MessageSource> Sources { get; set; } = [];    }
}
