using Client.IO;
using Client.Networking.Core;
using Client.Networking.Models;
using Client.Networking.Packets;
using Client.Utility;
using Clinet.IO;
using System.Diagnostics.CodeAnalysis;

namespace Client.Models.Chat;

public class ChatMessageBody
{
    [NotNull]
    public ChatMessage Parent { get; }

    public bool IsText { get; }

    private string _content;

    public int MessageId { get; private set; } = -1;

    public string Content
    {
        get { return _content; }
        set { _content = value; NotifyPropertyChanged(); }
    }

    public ChatMessageBody(ChatMessage parent, string content, bool isImage, int messageId, bool loadFromCache = false)
    {
        IsText = isImage != true;

        _content = content;
        MessageId = messageId;

        Parent = parent;

        //Images have to be loaded by doom. Won't be rendered by itself
        if (isImage)
        {
            if (loadFromCache)
            {
                LoadImage();
            }
            else
            {
                NotifyPropertyChanged();
            }
        }
    }

    private void LoadImage()
    {
        string fileName = Content.Substring(8);
        byte[]? imageBuffer = Cache.ReadFileBytesCache(fileName);

        if (imageBuffer is null)
        {
            ChatImagePacket packet = new ChatImagePacket(Content, "jpeg");
            SocketCore.Send(packet, Token.CHAT_IMAGE_REQUEST);
            ImageRequestQueue.AddRequest(packet, MessageId, LoadImageCallback);
        }
        else
        {
            string base64 = Convert.ToBase64String(imageBuffer);
            Content = string.Format("data:image/gif;base64,{0}", base64);
        }
    }

    public void LoadImageCallback(object packet)
    {
        byte[] buffer = Convert.FromBase64String(Convert.ToString(packet));

        //Content contains file name as for now
        Cache.SaveToCache(buffer, Content.Substring(8));

        Content = AvatarManager.ToJSImageSource(buffer);
    }

    private void NotifyPropertyChanged()
    {
        Parent.NotifyPropertyChanged();
    }

    public void SetMessageId(int id)
    {
        MessageId = id;
    }
}