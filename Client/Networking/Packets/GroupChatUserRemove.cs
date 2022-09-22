using Newtonsoft.Json;

namespace Client.Networking.Packets;

internal class GroupChatUserRemovePacket
{
    [JsonProperty("group_id")]
    public int GroupId { get; set; }
    [JsonProperty("removed_person_id")]
    public int RemovedPersonId { get; set; }

    public GroupChatUserRemovePacket(int groupId, int personId)
    {
        RemovedPersonId = personId;
        GroupId = groupId;
    }
}