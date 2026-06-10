namespace Rag.Blazor.Models;

public class ConversationDto
{
    public Guid ConversationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MessageCount { get; set; }
}
