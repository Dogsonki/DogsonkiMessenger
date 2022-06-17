using Newtonsoft.Json;

namespace Client.Models;

public class SearchModel
{
    [JsonProperty("login")]
    public string Username { get; set; }
    [JsonProperty("login_id")]
    public uint ID { get; set; }

    public SearchModel(string username, uint id)
    {
        Username = username;
        ID = id;
    }
}