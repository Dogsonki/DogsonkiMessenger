using Client.Networking.Packets;

namespace Client.Networking.Models;

internal class ImageRequestModel
{
    public ChatImagePacket Packet { get; set; }
    public int MessageId { get; set; }
    public Action<object> Callback { get; set; }

    public ImageRequestModel(ChatImagePacket packet, int messageId, Action<object> callback)
    {
        Packet = packet;
        MessageId = MessageId;
        Callback = callback;
    }
}