using Client.Utility;
using Newtonsoft.Json;

namespace Client.Networking.Packets.Models;

[Serializable]
public class SearchModel
{
    [JsonProperty("login")]
    public string Username { get; set; }
    [JsonProperty("login_id")]
    public int Id { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("last_message_time")]
    public DateTime LastMessageTime { get; set; }

    public bool isGroup => Type != "user";

    [JsonConstructor]
    public SearchModel(string name, int id, string type, double last_message_time)
    {
        Username = name;
        Id = id;
        Type = type;
        LastMessageTime = Essential.UnixToDateTime(last_message_time);
    }
}