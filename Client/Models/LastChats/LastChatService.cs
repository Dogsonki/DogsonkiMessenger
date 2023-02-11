namespace Client.Models.LastChats;

/// <summary>
/// Singelton service for last chats
/// </summary>
internal class LastChatService
{
    private static readonly List<LastChat> lastChats = new List<LastChat>();

    public static List<LastChat> GetLastChats() => lastChats;

    private static object padlock = new object();

    public static void UpdateLastMessage(IViewBindable lastChat, IViewBindable lastMessageOwner, string newMessage, double time, MessageType messageType)
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

    public static void AddLastMessage(IViewBindable lastChatBind, IViewBindable lastMessageOwner, string message, double time, MessageType messageType)
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
}