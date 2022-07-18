using Newtonsoft.Json;

namespace Client.Models;

[Serializable]
public class SearchModel
{
    [JsonProperty("login")]
    public string Username { get; set; }
    [JsonProperty("login_id")]
    public uint ID { get; set; }

    [JsonConstructor]
    public SearchModel(string username, uint id)
    {
        Username = username;
        ID = id;
    }
}