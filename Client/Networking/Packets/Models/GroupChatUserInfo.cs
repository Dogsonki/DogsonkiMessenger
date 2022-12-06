using Newtonsoft.Json;

namespace Client.Networking.Packets.Models;

[Serializable]
internal class GroupChatUserInfo
{
    [JsonProperty("is_admin")]
    public bool IsAdmin { get; set; }
    [JsonProperty("nick")]
    public string UserName { get; set; }
    [JsonProperty("user_id")]
    public uint UserId { get; set; }

    [JsonConstructor]
    public GroupChatUserInfo(bool is_admin, string nick, uint user_id)
    {
        IsAdmin = is_admin;
        UserName = nick;
        UserId = user_id;
    }
}