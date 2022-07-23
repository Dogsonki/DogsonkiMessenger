using Newtonsoft.Json;

namespace Client.Models;

[Serializable]
public class SearchModel
{
    [JsonProperty("login")]
    public string Username { get; set; }
    [JsonProperty("login_id")]
    public int Id { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonConstructor]
    public SearchModel(string name, int id,string type)
    {
        Username = name;
        Id = id;
        Type = type;
    }
}