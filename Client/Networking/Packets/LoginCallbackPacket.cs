using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
public class LoginCallbackPacket
{
    [JsonProperty("nick")]
    public string Username { get; set; }
    [JsonProperty("login_id")]
    public uint Id { get; set; }
    [JsonProperty("token")]
    public int Token { get; set; }
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonConstructor]
    public LoginCallbackPacket(string nick, uint? login_id, int token, string email)
    {
        Username = nick;

        if (login_id != null)
            Id = (uint)login_id;
        else
            Id = 0;

        Token = token;
        Email = email;
    }
}