using Newtonsoft.Json;

namespace Client.Models;

public class GroupCallbackModel
{
    [JsonProperty("group_name")]
    public string GroupName { get; set; }

    [JsonProperty("id")]
    public int GroupId { get; set; }

    [JsonConstructor]
    public GroupCallbackModel(string group_name, int id)
    {
        GroupName = group_name;
        GroupId = id;
    }
}