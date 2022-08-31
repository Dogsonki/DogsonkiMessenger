using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
internal class GroupChatCreatePacket
{
    [JsonProperty("group_name")]
    public string GroupName { get; set; }
    [JsonProperty("creator")]
    public int Creator { get; set; }
    [JsonProperty("users")]
    public int[] Users { get; set; }

    [JsonConstructor]
    public GroupChatCreatePacket(string group_name, int creator, int[] users) 
    {
        GroupName = group_name;
        Creator = creator;
        Users = users;
    }
}