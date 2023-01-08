using Client.Models;
using Client.Models.Chat;
using Client.Models.Exceptions;

public class ChatMessage
{
    private bool _isBeingChanged { get; set; }

    public IViewBindable AuthorView { get; init; }

    public DateTime Time { get; init; }

    public long TimeInTicks
    {
        get => Time.Ticks;
    }

    public List<ChatMessageBody> ChatMessageBodies { get; }

    private Action PropertyHasChanged { get; }

    public string FactoredTime
    {
        get
        {
            if (Time.Day == DateTime.Now.Day) return $"Today at {Time.Hour}:{Time.Minute}";
            else if (Time.Day == DateTime.Now.Day - 1) return $"Yesterday at {Time.Hour}:{Time.Minute}";
            else return $"{Time.Day}/{Time.Month}/{Time.Year} at {Time.Hour}:{Time.Minute}";
        }
    }

    public ChatMessage(string content, bool isImage, Action StateChanged)
    {
        ChatMessageBodies = new List<ChatMessageBody>();

        PropertyHasChanged = StateChanged;

        AuthorView = LocalUser.CurrentUser;

        ChatMessageBody body = new ChatMessageBody(this, content, isImage, 0);

        ChatMessageBodies.Add(body);

        Time = DateTime.Now;
    }

    public ChatMessage(string content, bool isImage, int messageId, Action StateChanged, int userId, DateTime time)
    {
        ChatMessageBodies = new List<ChatMessageBody>();

        PropertyHasChanged = StateChanged;

        AuthorView = User.GetUser((uint)userId);

        if(AuthorView is null)
        {
            throw new UserMemoryException("User is given id dose not exists in memory");
        }

        ChatMessageBody body;

        if (isImage)
        {
            body = new ChatMessageBody(this, content, true, messageId, true);
        }
        else
        {
            body = new ChatMessageBody(this, content, false, messageId);
        }

        ChatMessageBodies.Add(body);

        Time = time;
    }

    public void Append(string content, bool isImage)
    {
        ChatMessageBody body = new ChatMessageBody(this, content, isImage, 0);
        ChatMessageBodies.Add(body);
        NotifyPropertyChanged();
    }

    public void NotifyPropertyChanged()
    {
        PropertyHasChanged?.Invoke();
    }
}