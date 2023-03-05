using Client.Utility;
using System.ComponentModel;
using System.Text;

namespace Client.Models.LastChats;

public class LastChat : IViewBindable
{
    /// <summary>
    /// Returns view of this group/user
    /// </summary>
    public IViewBindable BindedView { get; }

    /// <summary>
    /// Returns view of this last message
    /// </summary>
    public IViewBindable View => this;

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

    public BindableType BindType => BindableType.Any;

    /// <summary>
    /// Name of this user/group
    /// </summary>
    public string Name => BindedView.Name;

    /// <summary>
    /// Id of this user/group
    /// </summary>
    public uint Id => BindedView.Id;

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
    /// Avatar of this user/group
    /// </summary>
    public string AvatarImageSource
    {
        get
        {
            return BindedView.AvatarImageSource;
        }
        set
        {
            PropertyChanged?.Invoke(this, null);

            BindedView.AvatarImageSource = value;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public LastChat(IViewBindable view, string? messageSender, MessageType? messageType, byte[]? message, double? messageTime, UserStatus status, 
        FriendStatus friendStatus = FriendStatus.Unknown)
    {
        BindedView = view;

        if (BindedView.BindType == BindableType.User)
        {
            ((User)BindedView.View).UserProperties.IsFriend = friendStatus;
        }

        MessageSenderName = messageSender;

        Status = status;

        Message = GetDecodedMessage(messageType, message);

        FactoredTime = GetFactoredTime(messageTime);
    }

    public LastChat(IViewBindable sender, string? messageSender, MessageType? messageType, string? message, double? messageTime, UserStatus status)
    {
        BindedView = sender;

        Status = status;

        MessageSenderName = messageSender;

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

    public void SilentDispose()
    {
        PropertyChanged -= PropertyChanged;
    }

    public string GetStatusColor()
    {
        if (Status == UserStatus.Online)
        {
            return "red";
        }
        else //There will be more statuses
        {
            return "green";
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
        PropertyChanged?.Invoke(this, null);
    }
}

public enum MessageType
{
    Image,
    Text
}