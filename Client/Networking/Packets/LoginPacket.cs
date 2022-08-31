using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
public class LoginPacket
{
    [JsonProperty("login")]
    public string Login { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
    [JsonProperty("remember")]
    public bool Remember { get; set; }

    [JsonConstructor]
    public LoginPacket(string login, string password, bool remember)
    {
        Login = login;
        Password = password;
        Remember = remember;
    }
}