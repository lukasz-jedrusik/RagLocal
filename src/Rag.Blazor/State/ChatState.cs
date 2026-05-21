using Rag.Blazor.Models;

namespace Rag.Blazor.State;

public class ChatState
{
    public List<ChatMessageModel> Messages { get; } = new();

    public Guid? ConversationId { get; set; }

    public bool IsLoading { get; set; }

    public void AddUserMessage(string text)
    {
        Messages.Add(new ChatMessageModel
        {
            Text = text,
            IsUser = true
        });
    }
}