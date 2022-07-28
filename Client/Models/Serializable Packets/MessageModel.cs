using Client.Models.UserType.Bindable;
using Client.Utility;
using Newtonsoft.Json;

namespace Client.Models;

[Serializable]
public class MessageModel
{
    public User BindedUser { get; set; }
    public string MessageContent { get; set; }
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
    public int UserId { get; set; }
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
