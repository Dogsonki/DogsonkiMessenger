using System.Text;
using Client.Utility;
using Newtonsoft.Json;

namespace Client.Networking.Packets;

public class MessagePacket
{
    [JsonProperty("message")]
    public byte[] Content { get; }

    [JsonProperty("message_type")]
    public string MessageType { get; }

    [JsonProperty("is_bot")]
    public bool IsBot { get; }

    [JsonIgnore]
    public bool WasSeen { get; }

    [JsonIgnore]
    public string ContentString { get; }

    [JsonIgnore]
    public bool IsImage
    {
        get
        {
            return MessageType != "text";
        }
    }

    [JsonIgnore]
    public bool IsGroup { get; }

    [JsonIgnore]
    public int GroupId { get; }

    [JsonIgnore]
    public int? UserId { get; }

    [JsonIgnore]
    public string Username { get; }

    [JsonIgnore]
    public DateTime Time { get; }

    [JsonIgnore]
    public int MessageId { get; }

    [JsonConstructor]
    public MessagePacket(string username, byte[] message, string message_type, double time, int user_id,
        bool is_group, int group_id, int id, bool seen, bool is_bot)
    {
        ContentString = Encoding.UTF8.GetString(message);
        Content = message;
        MessageType = message_type;
        Time = Essential.UnixToDateTime(time);
        UserId = user_id;
        IsGroup = is_group;
        GroupId = group_id;
        Username = username;
        MessageId = id;
        WasSeen = seen;
        IsBot = is_bot;
    }

    public MessagePacket(byte[] imageBuffer, string extension)
    {
        MessageType = extension;
        Content = imageBuffer;
    }

    public MessagePacket(string message)
    {
        MessageType = "text";
        Content = Encoding.UTF8.GetBytes(message);
    }
}
