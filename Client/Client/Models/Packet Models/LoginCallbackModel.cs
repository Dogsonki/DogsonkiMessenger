using Newtonsoft.Json;

namespace Client.Models
{
    public class LoginCallbackModel
    {
        [JsonProperty("login")]
        public string Username { get; set; }
        [JsonProperty("login_id")]
        public uint ID { get; set; }
        [JsonProperty("token")]
        public int Token { get; set; }

        [JsonConstructor]
        public LoginCallbackModel(string username, uint id, int token)
        {
            Username = username;
            ID = id;
            Token = token;
        }
    }
}
