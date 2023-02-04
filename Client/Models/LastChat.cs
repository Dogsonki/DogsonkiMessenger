using Client.Utility;
using System.ComponentModel;
using System.Text;

namespace Client.Models;

public class LastChat : IViewBindable
{
    public IViewBindable BindedView { get; }

    public IViewBindable View => this;

    public string? MessageSenderName { get; } = null;

    public BindableType BindType => BindableType.Any;

    public string Name => BindedView.Name;

    public uint Id => BindedView.Id;

    public string AvatarPath => BindedView.AvatarPath;

    private UserStatus status;

    public UserStatus Status
    {
        get
        {
            if(BindType == BindableType.User)
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

    public string? Message;

    public string? FactoredTime { get; init; }

    public LastChat(IViewBindable view, string? messageSender, string? messageType, byte[]? message, double? messageTime, UserStatus status)
    {
        BindedView = view;

        MessageSenderName = messageSender;

        Status = status;

        Message = GetDecodedMessage(messageType, message);

        FactoredTime = GetFactoredTime(messageTime);
    }

    public LastChat(IViewBindable sender, string? messageSender, string? messageType, string? message, double? messageTime, UserStatus status)
    {
        BindedView = sender;

        Status = status;

        MessageSenderName = messageSender;

        if(messageType == "text")
        {
            Message = message;
        }
        else
        {
            Message = "image";
        }

        FactoredTime = GetFactoredTime(messageTime);
    }

    private string GetDecodedMessage(string? messageType, byte[]? message)
    {
        if(messageType is null || message is null)
        {
            return string.Empty;
        }

        if (messageType == "text")
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
        if(messageTime is null)
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
        if(Status == UserStatus.Online)
        {
            return "red";
        }
        else //There will be more statuses
        {
            return "green";
        }
    }
}