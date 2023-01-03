using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
public class LoginPacket
{
    [JsonProperty("login")]
    public string Email { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
    [JsonProperty("remember")]
    public bool Remember { get; set; }

    [JsonConstructor]
    public LoginPacket(string email, string password, bool remember)
    {
        Email = email;
        Password = password;
        Remember = remember;
    }
}