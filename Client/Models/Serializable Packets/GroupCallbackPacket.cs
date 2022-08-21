using Newtonsoft.Json;

namespace Client.Models;

public class GroupCallbackPacket
{
    [JsonProperty("group_name")]
    public string GroupName { get; set; }

    [JsonProperty("id")]
    public int GroupId { get; set; }

    [JsonConstructor]
    public GroupCallbackPacket(string group_name, int id)
    {
        GroupName = group_name;
        GroupId = id;
    }
}