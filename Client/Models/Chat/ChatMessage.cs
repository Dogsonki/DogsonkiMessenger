using Client.Models;
using Client.Models.Chat;
using Client.Utility;

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

    public ChatMessage(string content, bool isImage, Action StateChanged)
    {
        ChatMessageBodies = new List<ChatMessageBody>();

        PropertyHasChanged = StateChanged;

        AuthorView = LocalUser.CurrentUser;

        ChatMessageBody body = new ChatMessageBody(this, content, isImage, 0);

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
            body = new ChatMessageBody(this, content, isImage:true, messageId, loadFromCache:true);
        }
        else
        {
            body = new ChatMessageBody(this, content, isImage:false, messageId);
        }

        ChatMessageBodies.Add(body);

        Time = time;
    }

    public void Append(string content, bool isImage, int messageId)
    {
        ChatMessageBody body = new ChatMessageBody(this, content, isImage, messageId);

        ChatMessageBodies.Insert(0, body);

        NotifyPropertyChanged();
    }

    public void NotifyPropertyChanged()
    {
        PropertyHasChanged?.Invoke();
    }
}