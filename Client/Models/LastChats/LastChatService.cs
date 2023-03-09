namespace Client.Models.LastChats;

/// <summary>
/// Singelton service for last chats
/// </summary>
internal class LastChatService
{
    private static readonly List<LastChat> lastChats = new List<LastChat>();

    public List<LastChat> GetLastChats() => lastChats;

    public int ChatsCount => lastChats.Count;

    private static object padlock = new object();

    public void UpdateLastChatMessage(IViewBindable lastChat, IViewBindable lastMessageOwner, string newMessage, double time, MessageType messageType)
    {
        lock (padlock)
        {
            LastChat? lastChatRef = lastChats.Find(x => x.BindedView.Id == lastChat.Id);

            if (lastChatRef is not null)
            {
                lastChatRef.UpdateLastMessage(lastMessageOwner, newMessage, time, messageType);
            }
        }
    }

    public void AddLastChat(IViewBindable lastChatBind, IViewBindable lastMessageOwner, string message, double time, MessageType messageType)
    {
        lock (padlock)
        {
            if (lastChats.Find(x => x.View.Id == lastChatBind.Id) is not null)
            {
                return;
            }

            LastChat lastChat = new LastChat(lastChatBind, lastMessageOwner.Name, messageType, message, time, UserStatus.None);

            lastChats.Add(lastChat);
        }
    }

    public void AddLastChat(IViewBindable view, IViewBindable messageAuthor, MessageType messageType, string? message) 
    {
        if(lastChats.Find(x=> x.Id == view.Id) == null) {

            UserStatus status = UserStatus.None;    

            if(view.BindType == BindableType.User){
                status = ((User)view).UserProperties.Status;
            }

            LastChat lastChat = new LastChat(view, view.Name, messageType, message, DateTime.Now.Ticks, status);
            lastChats.Add(lastChat);    
        }
    }

    public void AddLastChat(LastChat lastChat)
    {
        lock (padlock)
        {
            if (lastChats.Find(x => x.View.Id == lastChat.Id) is not null)
            {
                return;
            }

            lastChats.Add(lastChat);
        }
    }
}