using Client.Networking.Models;
using Client.Networking.Packets;

namespace Client.Networking.Core;

public static class ImageRequestQueue
{
    private static readonly List<ImageRequestModel> _queue = new List<ImageRequestModel>();

    public static void AddRequest(ChatImagePacket packet, uint messageId, Action<object> callback)
    {
        ImageRequestModel request = new ImageRequestModel(packet, messageId, callback);
        _queue.Add(request);

        RequestImageCallback(request,callback);
    }

    public static void RemoveRequest(uint messageId)
    {
        ImageRequestModel? model = _queue.Find(x => x.MessageId == messageId);

        if (model is null) return;

        _queue.Remove(model);

        if (_queue.Count > 0)
        {
            RequestImageCallback(_queue.Last(), model.Callback);
        }
    }

    private static void RequestImageCallback(ImageRequestModel model, Action<object> callback)
    {
        if (!RequestedCallback.IsAlreadyQueued(Token.CHAT_IMAGE_REQUEST))
        {
            SocketCore.SendCallback(model.Packet, Token.CHAT_IMAGE_REQUEST, callback);
        }
    }
}

