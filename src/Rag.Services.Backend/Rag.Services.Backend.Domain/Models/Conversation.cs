namespace Rag.Services.Backend.Domain.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public string ConversationId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public User User { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = [];
    }
}
