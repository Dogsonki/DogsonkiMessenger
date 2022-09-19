using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
internal class GroupInvitePacket
{
    [JsonProperty("group_id")]
    public int GroupId { get; set; }
    [JsonProperty("added_person_id")]
    public int AddedPersonId { get; set; }

    public GroupInvitePacket(int groupId, int personId)
    {
        AddedPersonId = personId;
        GroupId = groupId;
    }
}