using Client.Utility;
using System.ComponentModel;
using System.Text;

namespace Client.Models;

public class LastChat : IViewBindable
{
    public IViewBindable BindedView { get; }

    public IViewBindable View => this;

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
            PropertyChanged?.Invoke(this, null);

            return BindedView.AvatarImageSource;
        }
        set
        {
            BindedView.AvatarImageSource = value;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? Message;

    public string? FactoredTime { get; init; }

    public LastChat(IViewBindable sender, string? messageType, byte[]? message, double? messageTime, UserStatus status)
    {
        BindedView = sender;
        Status = status;

        Message = GetDecodedMessage(messageType, message);

        FactoredTime = GetFactoredTime(messageTime);
    }

    public LastChat(IViewBindable sender, string? messageType, string? message, double? messageTime, UserStatus status)
    {
        BindedView = sender;
        Status = status;
        
        if(messageType is not null && message is not null)
        {
            if (messageType == "text")
            {
                Message = message;
            }
            else
            {
                Message = "Image";
            }
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

        DateTime time = Essential.UnixToDateTime((double)messageTime);

        if (time.Day == DateTime.Now.Day) return $"{time.Hour}:{time.Minute}";
        else if (time.Day == DateTime.Now.Day - 1) return $"Yesterday";
        else return $"{time.Day}/{time.Month}/{time.Year}";
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