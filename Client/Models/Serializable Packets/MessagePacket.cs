using Client.Utility;
using Newtonsoft.Json;

namespace Client.Models;

public class MessagePacket
{
    public string Content;

    public bool IsGroup;
    public int GroupId;

    public int UserId;
    public string Username;

    [JsonProperty("message_type")]
    public string MessageType;

    public DateTime Time;

    [JsonConstructor]
    public MessagePacket(string username, string message, string message_type, double time, int user_id, bool is_group, int group_id)
    {
        Content = message;
        MessageType = message_type;
        Time = Essential.UnixToDateTime(time);
        UserId = user_id;
        IsGroup = is_group;
        GroupId = group_id;
        Username = username;
    }

    public MessagePacket(byte[] imageBuffer,string extension)
    {
        MessageType = extension;
        Content = string.Join("", imageBuffer);
    }
}
