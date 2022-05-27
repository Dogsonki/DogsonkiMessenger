using Newtonsoft.Json;

namespace Client.Models
{
    public class RegisterModel
    {
        [JsonProperty("login")]
        public string Login { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }

        public RegisterModel(string login,string password)
        {
            Login = login;
            Password = password;
        }
    }
}
