using Newtonsoft.Json;

namespace Client.Models;

[Serializable]
public class LoginCallbackModel
{
    [JsonProperty("nick")]
    public string Username { get; set; }
    [JsonProperty("login_id")]
    public string ID { get; set; }
    [JsonProperty("token")]
    public string Token { get; set; }
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonConstructor]
    public LoginCallbackModel(string nick, string login_id, string token, string email)
    { 
        Username = nick;
        ID = login_id;
        Token = token;
        Email = email;
    }
}