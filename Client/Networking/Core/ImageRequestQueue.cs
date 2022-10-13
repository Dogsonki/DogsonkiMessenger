using Client.Models.Bindable;
using Client.Networking.Models;
using Client.Networking.Packets;

namespace Client.Networking.Core;

internal static class ImageRequestQueue
{
    private static readonly List<ImageRequestModel> _queue = new List<ImageRequestModel>();

    public static void AddRequest(ChatImagePacket packet, ChatMessage message)
{                                           
        ImageRequestModel request = new ImageRequestModel(packet, message);
        _queue.Add(request);

        RequestImageCallback(request);
    }

    public static void RemoveRequest(ChatMessage message)
    {
        ImageRequestModel? model = _queue.Find(x => x.Message == message);
        if(model is not null) _queue.Remove(model);

        if (_queue.Count > 0)
        {
            RequestImageCallback(_queue.Last());
        }
    }

    private static void RequestImageCallback(ImageRequestModel model)
    {
        if (!RequestedCallback.IsAlreadyQueued(Token.CHAT_IMAGE_REQUEST))
        {
            SocketCore.SendCallback(model.Message.GetImage, model.Packet, Token.CHAT_IMAGE_REQUEST);
        }
    }
}

