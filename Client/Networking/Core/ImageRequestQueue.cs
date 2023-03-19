using Client.Networking.Models;
using Client.Networking.Packets;
using Client.Pages;
using Client.Utility;

namespace Client.Networking.Core;

public static class ImageRequestQueue
{
    private static readonly List<ImageRequestModel> _queue = new List<ImageRequestModel>();

    public static void AddRequest(ChatImagePacket packet, int messageId, Action<object> callback)
    {
        ImageRequestModel request = new ImageRequestModel(packet, messageId, callback);

        _queue.Add(request);

        RequestImageCallback(request,callback);
    }

    public static void RemoveRequest(int messageId)
    {
        ImageRequestModel? model = _queue.Find(x => x.MessageId == messageId);

        if (model is null) return;

        _queue.Remove(model);

        if (_queue.Count > 0)
        {
            RequestImageCallback(_queue.First(), model.Callback);
        }
    }

    private static void RequestImageCallback(ImageRequestModel model, Action<object> callback)
    {
        if (!RequestedCallback.IsAlreadyQueued(Token.CHAT_IMAGE_REQUEST))
        {
            SocketCore.SendCallback(model.Packet, Token.CHAT_IMAGE_REQUEST, LoadImageInvocator);
        }
    }

    private static void LoadImageInvocator(object image) 
    {
        ImageRequestModel model = _queue.First();
        
        foreach(var chatMessage in ChatPage.Messages) 
        {
            var body = chatMessage.ChatMessageBodies.Find(x => x.MessageId == model.MessageId);
            
            if(body is not null) {
                body.LoadImageCallback(image);
                break;
            }
        }

        RemoveRequest(model.MessageId);
    }
}

