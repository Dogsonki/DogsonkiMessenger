using Client.Utility;
using Newtonsoft.Json;

namespace Client.Networking.Packets.Models;

[Serializable]
public class SearchModel
{
    [JsonProperty("login")]
    public string Name { get; set; }
    [JsonProperty("login_id")]
    public uint Id { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("last_message_time")]
    public DateTime LastMessageTime { get; set; }

    public bool isGroup => Type != "user";

    [JsonConstructor]
    public SearchModel(string name, uint id, string type, double last_message_time)
    {
        Name = name;
        Id = id;
        Type = type;
        LastMessageTime = Essential.UnixToDateTime(last_message_time);
    }
}