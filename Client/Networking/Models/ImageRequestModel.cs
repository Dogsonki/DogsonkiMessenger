using Client.Models.UserType.Bindable;
using Client.Networking.Packets;

namespace Client.Networking.Models;

internal class ImageRequestModel
{
    public ChatImagePacket Packet;
    public ChatMessage Message;

    public ImageRequestModel(ChatImagePacket packet, ChatMessage message)
    {
        Packet = packet;
        Message = message;
    }
}