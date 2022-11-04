using Client.Utility;
using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
internal class GroupImageRequestPacket
{
    [JsonProperty("avatar")]
    public byte[] ImageData { get; set; }
    [JsonProperty("group_id")]
    public int GroupId { get; set; }

    public GroupImageRequestPacket(byte[] avatar, int groupId)
    {
        ImageData = avatar;
        GroupId = groupId;
    }

    [JsonConstructor]
    public GroupImageRequestPacket(string avatar, int group_id)
    {
        ImageData = Essential.GetImageBuffer(avatar);
        GroupId = group_id;

        Debug.Write("bfcount"+ImageData.Length);
    }
}