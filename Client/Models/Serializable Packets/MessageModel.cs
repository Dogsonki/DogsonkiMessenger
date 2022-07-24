using Client.Models.UserType.Bindable;
using Client.Utility;
using Newtonsoft.Json;

namespace Client.Models;

[Serializable]
public class MessageModel
{
    public string MessageContent { get; set; }
    public string Username { get; set; }
    public ImageSource AvatarImage { get; set; }
    public DateTime Time { get; set; }
    public int UserId { get; set; }
    public int GroupId { get; set; }
    public bool IsGroup { get; set; }

    //Used by server
    [JsonConstructor]
    public MessageModel(string user, string message, double time, int user_id, bool is_group, int group_id)
    {
        Username = user;
        MessageContent = message;
        AvatarImage = User.CreateOrGet(user, user_id).Avatar;
        GroupId = group_id;
        IsGroup = is_group;

        Debug.Write($"Settings avatar: {Username} {AvatarImage.Id} {user_id}");
        UserId = user_id;

        try
        {
            Time = Essential.UnixToDateTime(time);
        }
        catch (Exception ex)
        {
            Debug.Error($"Cannot parse time {time}" + ex);
        }
    }

    //Used by client
    public MessageModel(string user, string message, DateTime time)
    {
        Username = user;
        MessageContent = message;
        Time = time;
        AvatarImage = LocalUser.Current.Avatar;
    }
}
