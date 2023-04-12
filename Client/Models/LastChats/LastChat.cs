using Client.IO.Models.Offline;
using Client.Utility;
using System.Text;

namespace Client.Models.LastChats;

public class LastChat : ViewBindable
{
    /// <summary>
    /// Returns view of this group/user
    /// </summary>
    public IViewBindable BindedView { get; }

    /// <summary>
    /// Name of user that sent last message
    /// </summary>
    public string? MessageSenderName { get; private set; } = null;

    /// <summary>
    /// Last message
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// FactoredTime of last message
    /// </summary>
    public string? FactoredTime { get; private set; }

    private UserStatus status;

    /// <summary>
    /// Status of user/group ex. Online/Offline
    /// </summary>
    public UserStatus Status
    {
        get
        {
            if (BindType == BindableType.User)
            {
                return status;
            }

            return UserStatus.None;
        }
        set
        {
            status = value;
        }
    }

    /// <summary>
    /// Time of last message in timestamp
    /// </summary>
    public double? LastMessageTimestamp { get; private set; }   


    public LastChat(IViewBindable view, string? messageSender, MessageType? messageType, byte[]? message, double? messageTime, UserStatus status, 
        FriendStatus friendStatus = FriendStatus.Unknown) : base(BindableType.Any, view.Name, view.Id)
    {
        BindedView = view;

        MessageSenderName = messageSender;

        Status = status;

        Message = GetDecodedMessage(messageType, message);

        FactoredTime = GetFactoredTime(messageTime);

        LastMessageTimestamp = messageTime;

        if (view.IsUser())
        {
            ((User)view).UserProperties.IsFriend = friendStatus;    
        }
    }

    public LastChat(IViewBindable sender, string? messageSender, MessageType? messageType,
        string? message, double? messageTime, UserStatus status) : base(BindableType.Any, sender.Name, sender.Id)
    {
        BindedView = sender;

        Status = status;

        MessageSenderName = messageSender;

        LastMessageTimestamp = messageTime;

        if (messageType == MessageType.Text)
        {
            Message = message;
        }
        else
        {
            Message = "image";
        }

        FactoredTime = GetFactoredTime(messageTime);
    }

    public LastChat(LastChatCache cachedLastChat) : base(BindableType.Any, cachedLastChat.ChatName, cachedLastChat.ChatId)
    {
        BindedView = IViewBindable.CreateOrGet(cachedLastChat.ChatName, cachedLastChat.ChatId, false);
        Status = UserStatus.Offline;
        MessageSenderName = cachedLastChat.LastMessageAuthorName;
        FactoredTime = GetFactoredTime(cachedLastChat.LastMessageTime);
        Message = cachedLastChat.LastMessage;

    }

    private string GetDecodedMessage(MessageType? messageType, byte[]? message)
    {
        if (messageType is null || message is null)
        {
            return string.Empty;
        }

        if (messageType == MessageType.Text)
        {
            return Encoding.UTF8.GetString(message);
        }
        else
        {
            return "Image";
        }
    }

    private string GetFactoredTime(double? messageTime)
    {
        if (messageTime is null)
        {
            return string.Empty;
        }

        return Essential.DateTimeToFactored((double)messageTime);
    }

    public string GetStatusColor()
    {
        if (Status == UserStatus.Offline)
        {
            return "#80848E";
        }
        else
        {
            return "#23A55A";
        }
    }

    public void UpdateLastMessage(IViewBindable messageOwner, string message, double time, MessageType messageType)
    {
        if (messageType == MessageType.Text)
        {
            Message = message;
        }
        else
        {
            Message = "Image";
        }

        FactoredTime = Essential.DateTimeToFactored(time);
        MessageSenderName = messageOwner.Name;
    }
}

public enum MessageType
{
    Image,
    Text,
}