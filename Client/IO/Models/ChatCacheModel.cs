using Newtonsoft.Json;

namespace Client.IO.Models;

[Serializable]
public class ChatCacheModel
{
    [JsonProperty("messages")]
    public ChatMessageCacheModel[] Messages;
    [JsonProperty("last_message_time")]
    public double LastMessageTime;

    [JsonConstructor]
    public ChatCacheModel(IEnumerable<ChatMessageCacheModel> messages, double last_message_time)
    {
        Messages = messages.ToArray();
        LastMessageTime = last_message_time;
    }
}