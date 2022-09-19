using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
public class GroupChatCreateCallbackPacket
{
    [JsonProperty("group_name")]
    public string GroupName { get; set; }
    [JsonProperty("group_id")]
    public int GroupId { get; set; }

    [JsonConstructor]
    public GroupChatCreateCallbackPacket(string group_name, int group_id)
    {
        GroupName = group_name;
        GroupId = group_id;
    }
}