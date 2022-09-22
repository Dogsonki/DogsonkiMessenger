using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
internal class GroupChatUserInvitePacket
{
    [JsonProperty("group_id")]
    public int GroupId { get; set; }
    [JsonProperty("added_person_id")]
    public int AddedPersonId { get; set; }

    public GroupChatUserInvitePacket(int groupId, int personId)
    {
        AddedPersonId = personId;
        GroupId = groupId;
    }
}