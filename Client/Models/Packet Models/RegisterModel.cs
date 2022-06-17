using Newtonsoft.Json;

namespace Client.Models;

public class RegisterModel
{
    [JsonProperty("login")]
    public string Login { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
    [JsonProperty("email")]
    public string Email { get; set; }

    public RegisterModel(string login, string password,string email)
    {
        Login = login;
        Password = password;
        Email = email;
    }
}
