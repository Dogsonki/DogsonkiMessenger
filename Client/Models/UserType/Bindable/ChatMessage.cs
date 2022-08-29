using Client.Utility;
using System.ComponentModel;
using System.Text;

namespace Client.Models.UserType.Bindable;

[Bindable(BindableSupport.Yes)]
public class ChatMessage : BindableObject
{
    public User BindedUser { get; init; }
    
    private DateTime Time { get; init; }

    public bool IsImage { get; set; } = false;
    public bool IsText { get { return !IsImage; } }

    public ImageSource Avatar
    {
        get
        {
            return BindedUser.Avatar;
        }
    }

    public ImageSource? Image { get; init; }
    public string? TextContent { get; set; }

    public string FactoredTime
    {
        get
        {
            if (Time.Day == DateTime.Now.Day) return $"Today at {Time.Hour}:{Time.Minute}";
            else if (Time.Day == DateTime.Now.Day - 1) return $"Yesterday at {Time.Hour}:{Time.Minute}";
            else return $"{Time.Day}/{Time.Month}/{Time.Year} at {Time.Hour}:{Time.Minute}";
        }
    }

    public ChatMessage(User user,string message)
    {
        BindedUser = user;
        TextContent = message;
    }

    public ChatMessage(string message)
    {
        TextContent = message;
        BindedUser = LocalUser.UserRef;
    }

    public ChatMessage(ImageSource image)
    {
        Image = image;
        BindedUser = LocalUser.UserRef;
    } 

    public ChatMessage(MessagePacket packet)
    {
        BindedUser = User.CreateOrGet(packet.Username,packet.UserId);

        if (packet.MessageType == "text") 
        {
            TextContent = packet.Content;
        }
        else
        {
            Image = ImageSource.FromStream(() => new MemoryStream(Encoding.UTF8.GetBytes(packet.Content)));
            IsImage = true;
        }

        Time = packet.Time;
    }
}