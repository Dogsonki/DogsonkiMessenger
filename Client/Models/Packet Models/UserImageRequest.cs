using Newtonsoft.Json;

namespace Client.Models;

public class UserImageRequest
{
    [JsonProperty("avatar")]
    public string ImageData { get; set; }
    [JsonProperty("login_id")]
    public uint UserID { get; set; }

    public UserImageRequest(string avatar, uint login_id)
    {
        ImageData = avatar;
        UserID = login_id;
    }
}
