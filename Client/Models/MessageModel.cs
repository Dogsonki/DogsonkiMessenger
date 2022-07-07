using Newtonsoft.Json;

namespace Client.Models;

[Serializable]
public class MessageModel
{
    public string MessageContent { get; set; }
    public string Username { get; set; }
    public ImageSource AvatarImage { get; set; }
    public DateTime Time { get; set; }

    //Used by server
    [JsonConstructor]
    public MessageModel(string user, string message, double time, uint user_id)
    {
        Username = user;
        MessageContent = message;
        AvatarImage = User.CreateOrGet(user, user_id).Avatar;

        try
        {
            Time = UnixToDateTime(time);
        }
        catch (Exception ex)
        {
            Debug.Error($"Cannot parse time {time}" + ex);
        }
    }

    public static DateTime UnixToDateTime(double unixTimeStamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
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
