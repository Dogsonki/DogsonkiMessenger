using Client.IO;
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

    public string AvatarImageSource 
    {
        get
        {
            PropertyChanged?.Invoke(this, null);

            return BindedView.AvatarImageSource;
        }
        set
        {
            //Impossible setter
            BindedView.AvatarImageSource = value;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? Message;

    public string? FactoredTime { get; init; }

    public LastChat(IViewBindable sender, string? messageType, byte[]? message, double? messageTime)
    {
        BindedView = sender;

        if (message is not null)
        {
            if (messageType == "text")
            {
                Message = Encoding.UTF8.GetString(message);
            }
            else
            {
                Message = "Image";
            }
        }

        if (messageTime is not null)
        {
            DateTime time = Essential.UnixToDateTime((double)messageTime);

            if (time.Day == DateTime.Now.Day) FactoredTime = $"{time.Hour}:{time.Minute}";
            else if (time.Day == DateTime.Now.Day - 1) FactoredTime = $"Yesterday";
            else FactoredTime = $"{time.Day}/{time.Month}/{time.Year}";
        }
    }

    public void SilentDispose()
    {
        PropertyChanged -= PropertyChanged;
    }
}