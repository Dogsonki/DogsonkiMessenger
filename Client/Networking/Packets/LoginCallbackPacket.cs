using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
public class LoginCallbackPacket
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
    public LoginCallbackPacket(string nick, string login_id, string token, string email)
    {
        Username = nick;
        ID = login_id;
        Token = token;
        Email = email;
    }
}