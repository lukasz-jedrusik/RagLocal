namespace Rag.Services.Backend.Application.DataTransferObjects
{
    public class ConversationDto
    {
        public string ConversationId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int MessageCount { get; set; }
    }
}
