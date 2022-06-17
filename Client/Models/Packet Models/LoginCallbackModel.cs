using Newtonsoft.Json;

namespace Client.Models;

public class LoginCallbackModel
{
    [JsonProperty("login")]
    public string Username { get; set; }
    [JsonProperty("login_id")]
    public string ID { get; set; }
    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonConstructor]
    public LoginCallbackModel(string username, string id, string token)
    {
        Username = username;
        ID = id;
        Token = token;
    }
}