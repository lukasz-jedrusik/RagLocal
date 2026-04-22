namespace Rag.Services.Backend.Domain.Models
{
    public class ConversationMessage
    {
        public string Role { get; set; } = string.Empty; // "user", "assistant", or "system"
        public string Content { get; set; } = string.Empty;
    }
}
