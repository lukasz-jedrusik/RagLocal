namespace Rag.Blazor.Models;

public class ConversationDetailDto
{
    public Guid ConversationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
}

public class MessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Source> Sources { get; set; } = new();
}
