using Rag.Blazor.Models;

namespace Rag.Blazor.State;

public class ChatState
{
    public List<ChatMessageModel> Messages { get; } = new();

    public Guid? ConversationId { get; set; }

    public bool IsLoading { get; set; }

    public event Action? OnStateChanged;
    public event Action? OnConversationUpdated;

    public void AddUserMessage(string text)
    {
        Messages.Add(new ChatMessageModel
        {
            Text = text,
            IsUser = true
        });
        NotifyStateChanged();
    }

    public void LoadConversation(ConversationDetailDto conversation)
    {
        Messages.Clear();
        ConversationId = conversation.ConversationId;

        foreach (var msg in conversation.Messages)
        {
            Messages.Add(new ChatMessageModel
            {
                Text = msg.Content,
                IsUser = msg.Role == "user",
                Sources = msg.Sources?.Count > 0 ? msg.Sources : null
            });
        }

        NotifyStateChanged();
    }

    public void ClearConversation()
    {
        Messages.Clear();
        ConversationId = null;
        IsLoading = false;
        NotifyStateChanged();
    }

    public void NotifyConversationUpdated()
    {
        OnConversationUpdated?.Invoke();
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}