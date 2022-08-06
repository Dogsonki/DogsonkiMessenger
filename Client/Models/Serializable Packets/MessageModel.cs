using Client.Models.UserType.Bindable;
using Client.Utility;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Client.Models;

[Serializable]
[Bindable(BindableSupport.Yes)]
public class MessageModel : BindableObject
{
    public User BindedUser { get; set; }
    private string messageContent;
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

    public ImageSource AvatarImage
    {
        get
        {
            return BindedUser.Avatar;
        }
    }

    public DateTime Time { get; set; }

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

    public int GroupId { get; set; }
    public bool IsGroup { get; set; }

    //Used by server
    [JsonConstructor]
    public MessageModel(string user, string message, double time, int user_id, bool is_group, int group_id)
    {
        MessageContent = message;
        BindedUser = User.GetUser(user_id);
        GroupId = group_id;
        IsGroup = is_group;

        UserId = user_id;

        Time = Essential.UnixToDateTime(time);
    }

    //Used by client
    public MessageModel(string user, string message, DateTime time)
    {
        MessageContent = message;
        Time = time;
        BindedUser = LocalUser.UserRef;
    }
}
