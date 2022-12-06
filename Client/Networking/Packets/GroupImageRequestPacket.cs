using Client.Utility;
using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
internal class GroupImageRequestPacket
{
    [JsonProperty("avatar")]
    public byte[] ImageData { get; set; }
    [JsonProperty("group_id")]
    public uint GroupId { get; set; }

    public GroupImageRequestPacket(byte[] avatar, uint groupId)
    {
        ImageData = avatar;
        GroupId = groupId;
    }

    [JsonConstructor]
    public GroupImageRequestPacket(string avatar, uint group_id)
    {
        ImageData = Essential.GetImageBuffer(avatar);
        GroupId = group_id;
    }
}