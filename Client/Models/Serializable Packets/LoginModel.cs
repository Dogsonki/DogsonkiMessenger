using Newtonsoft.Json;

namespace Client.Models;

[Serializable]
public class LoginModel
{
    [JsonProperty("login")]
    public string Login { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
    [JsonProperty("remember")]
    public bool Remember { get; set; }

    [JsonConstructor]
    public LoginModel(string login, string password, bool remember)
    {
        Login = login;
        Password = password;
        Remember = remember;
    }
}