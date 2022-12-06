using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
internal class GroupChatCreatePacket
{
    [JsonProperty("group_name")]
    public string GroupName { get; set; }
    [JsonProperty("creator")]
    public uint Creator { get; set; }
    [JsonProperty("users")]
    public uint[] Users { get; set; }

    [JsonConstructor]
    public GroupChatCreatePacket(string group_name, uint creator, uint[] users) 
    {
        GroupName = group_name;
        Creator = creator;
        Users = users;
    }
}