using System.ComponentModel;
using Client.Utility;
using System.Text;
using Client.Pages;

namespace Client.Models.Bindable;

public class BindableLastChat : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly User? BindedUser;
    private readonly Group? BindedGroup;

    public readonly bool IsGroup;

    public ImageSource avatar;

    public ImageSource Avatar
    {
        get
        {
            return avatar;
        }
        set
        {
            avatar = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Avatar)));
        }
    }

    private bool HasLastMessage { get; init; } = false;

    public string Sender { get; init; }
    public string FactoredTime { get; init; }
    public string Message { get; init; }
    public string Id { get; init; }
    public string Name
    {
        get
        {
            if (IsGroup) return BindedGroup.Name;
            else return BindedUser.Username;
        }
    }
    private string MessageType { get; init; }
    public Command Input { get; init; }

    public static void AddLastChat(Group group) => MainPage.AddLastChat(group);
    public static void AddLastChat(User user) => MainPage.AddLastChat(user);

    public string FactoredLastMessage
    {
        get
        {
            if (HasLastMessage)
            {
                return $"{Sender}: {GetMessage()} {FactoredTime}";
            }
            else
            {
                return "";
            }
            
        }
    }

    public BindableLastChat(string name, string type, byte[] message, double? messageTime, string messageType, int id, string senderUsername)
    {
        if(message != null)
        {
            Message = Encoding.UTF8.GetString(message);
        }
        else
        {
            Message = string.Empty;
        }

        if (type == "user")
        {
            IsGroup = false;
            BindedUser = User.CreateOrGet(name, id);
            Input = new Command(() => Conversation.OpenChat(BindedUser));
        }
        else
        {
            IsGroup = true;
            BindedGroup = Group.CreateOrGet(name, id);
            Input = new Command(() => Conversation.OpenChat(BindedGroup));
            avatar = BindedGroup.Avatar;
        }

        //Chat doesn't have any messages
        if (messageTime is not null)
        {
            DateTime time = Essential.UnixToDateTime((double)messageTime);
            if(time.Day == DateTime.Now.Day) FactoredTime = $"{time.Hour}:{time.Minute}";
            else if (time.Day == DateTime.Now.Day - 1) FactoredTime = $"Yesterday";
            else FactoredTime = $"{time.Day}/{time.Month}/{time.Year}";

            Sender = senderUsername;
            HasLastMessage = true;
        }

        MessageType = messageType;
        Id = id.ToString();
    }

    public BindableLastChat(Group group)
    {
        IsGroup = true;
        BindedGroup = group;
        Input = new Command(() => Conversation.OpenChat(group));
        Id = group.Id.ToString();
    }

    public BindableLastChat(User user)
    {
        IsGroup = false;
        BindedUser = user;
        Input = new Command(() => Conversation.OpenChat(user));
        Id = user.Id.ToString();
    }

    private string GetMessage()
    {
        if (MessageType == "text")
        {
            return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Message));
        }
        else
        {
            return "Image";
        }
    }
}