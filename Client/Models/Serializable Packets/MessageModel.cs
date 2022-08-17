using Client.Models.UserType.Bindable;
using Client.Utility;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Client.Models;

[Serializable]
[Bindable(BindableSupport.Yes)]
public class MessageModel : BindableObject
{
    [JsonIgnore]
    public User BindedUser { get; set; }

    private string? messageContent;
    public string MessageContent
    {
        get { return messageContent; }
        set { messageContent = value; OnPropertyChanged(nameof(MessageContent)); }
    }

    public string Username
    {
        get
        {
            return BindedUser.Username;
        }
    } 

    [JsonIgnore]
    public ImageSource AvatarImage
    {
        get
        {
            return BindedUser.Avatar;
        }
    }

    private DateTime time;
    public DateTime Time
    {
        get 
        { 
            if(time == DateTime.MinValue) { return DateTime.Now; }
            else { return time; }
        }
        set { time = value; }
    }

    [JsonIgnore]
    public string FactoredTime
    {
        get
        {
            if(Time.Day == DateTime.Now.Day)
            {
                return $"Today at {Time.Hour}:{Time.Minute}";
            }
            else if(Time.Day == DateTime.Now.Day-1)
            {
                return $"Yesterday at {Time.Hour}:{Time.Minute}";
            }
            else
            {
                return $"{Time.Day}/{Time.Month}/{Time.Year} at {Time.Hour}:{Time.Minute}";
            }
        }
    }

    private int userId;
    public int UserId
    {
        get
        {
            if (BindedUser != null)
            {
                return BindedUser.Id;
            }
            else
            {
                return userId;
            }
        }
        set { userId = value; }
    }

    [JsonIgnore]
    public bool IsImageMessage { get; set; }
    [JsonIgnore]
    public bool IsContentMessage { get; set; }

    [JsonIgnore]
    public ImageSource? ImageMessage { get; set; }

    [JsonProperty("message_type")]
    public string MessageType { get; set; }

    public int GroupId { get; set; }
    public bool IsGroup { get; set; }

    [JsonIgnore]
    public Color TextColor
    {
        get
        {
            return Color.FromRgb(255,255,255);
        }
    }

    //Used by server
    [JsonConstructor]
    public MessageModel(string user, string message, string message_type, double time, int user_id, bool is_group, int group_id)
    {
        MessageContent = message;
        BindedUser = User.GetUser(user_id);

        if(BindedUser is null) { throw new Exception("Referencing null User in BindedUser"); }

        MessageType = message_type;

        GroupId = group_id;
        IsGroup = is_group;

        UserId = user_id;

        Time = Essential.UnixToDateTime(time);
    }

    //Used by client
    public MessageModel(string message)
    {
        BindedUser = LocalUser.UserRef;
        MessageContent = message;
        MessageType = "text";
        IsImageMessage = false;
        IsContentMessage = true;
    }

    //Creates message as other user, for tests
    public MessageModel(User user,string message)
    {
        BindedUser = user;
        MessageContent = message;
        MessageType = "text";
        IsImageMessage = false;
        IsContentMessage = true;
    }

    public MessageModel(byte[] imageBuffer,string extension)
    {
        BindedUser = LocalUser.UserRef;
        MessageContent = String.Join("", imageBuffer);
        MessageType = extension;
        IsImageMessage = true;
        IsContentMessage = false;
    }

    public MessageModel(ImageSource imageSrc)
    {
        BindedUser = LocalUser.UserRef;
        ImageMessage = imageSrc;
        MessageType = "image";
        IsImageMessage = true;
        IsContentMessage = false;
    }
}
