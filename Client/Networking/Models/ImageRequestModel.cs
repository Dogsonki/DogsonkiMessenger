using Client.Networking.Packets;

namespace Client.Networking.Models;

internal class ImageRequestModel
{
    public ChatImagePacket Packet { get; set; }
    public uint MessageId { get; set; }
    public Action<object> Callback { get; set; }

    public ImageRequestModel(ChatImagePacket packet, uint messageId, Action<object> callback)
    {
        Packet = packet;
        MessageId = MessageId;
        Callback = callback;
    }
}