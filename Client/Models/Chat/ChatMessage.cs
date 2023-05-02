using Client.Utility;

namespace Client.Models.Chat;

public class ChatMessage
{
    public IViewBindable AuthorView { get; init; }

    public DateTime Time { get; init; }

    public long TimeInTicks
    {
        get => Time.Ticks;
    }

    public List<ChatMessageBody> ChatMessageBodies { get; }

    private Action PropertyHasChanged { get; }

    public string FactoredTime => Essential.DateTimeToFactored(TimeInTicks);

    public ChatMessage(string content, MessageType type, Action stateChanged, string extension = "")
    {
        ChatMessageBodies = new List<ChatMessageBody>();

        PropertyHasChanged = stateChanged;

        AuthorView = LocalUser.CurrentUser;

        ChatMessageBody body = new ChatMessageBody(this, content, type, 0, extension);

        ChatMessageBodies.Add(body);

        Time = DateTime.Now;
    }

    public ChatMessage(string content, bool isImage, int messageId, Action StateChanged, 
        int userId, DateTime time, bool bot_response = false)
    {
        ChatMessageBodies = new List<ChatMessageBody>();
        
        PropertyHasChanged = StateChanged;

        if (!bot_response)
        {
            if(userId == LocalUser.CurrentUser.Id) {
                AuthorView = LocalUser.CurrentUser; 
            }
            else {
                AuthorView = User.GetUser((uint)userId);
            }
        }
        else 
        {
            AuthorView = User.SystemBot;
        }

        if (AuthorView is null)
        {
            throw new ArgumentException("User with given id dose not exist");
        }

        ChatMessageBody body;

        if (isImage)
        {
            body = new ChatMessageBody(this, content, MessageType.Image, messageId, loadFromCache:true);
        }
        else
        {
            body = new ChatMessageBody(this, content, MessageType.Text, messageId);
        }

        ChatMessageBodies.Add(body);

        Time = time;
    }

    public void Append(string content, MessageType type, int messageId)
    {
        ChatMessageBody body = new ChatMessageBody(this, content, type, messageId);

        ChatMessageBodies.Insert(0, body);

        NotifyPropertyChanged();
    }

    public void NotifyPropertyChanged()
    {
        PropertyHasChanged?.Invoke();
    }
}

public enum MessageType
{
    Image,
    Video,
    Text,
    File
}