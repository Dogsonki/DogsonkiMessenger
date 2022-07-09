using Newtonsoft.Json;

namespace Client.Models;

[Serializable]
internal class GroupChatCreateModel
{
    [JsonProperty("group_name")]
    public string GroupName { get; set; }
    [JsonProperty("creator")]
    public int Creator { get; set; }

    public GroupChatCreateModel(string group_name, int creator)
    {
        GroupName = group_name;
        Creator = creator;
    }
}