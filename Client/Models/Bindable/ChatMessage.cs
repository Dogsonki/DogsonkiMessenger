using Client.Networking.Core;
using System.ComponentModel;
using Client.Utility;
using Client.Networking.Packets;
using System.Text;
using Client.IO;
using Client.Networking.Models;
using Client.IO.Models;

namespace Client.Models.Bindable;

[Bindable(BindableSupport.Yes)]
public class ChatMessage : BindableObject
{
    public User BindedUser { get; init; }
    
    public DateTime Time { get; init; }

    public bool IsImage { get; } = false;

    public bool IsText { get; }

    public long TimeInTicks
    {
        get => Time.Ticks;
    }

    public ImageSource Avatar
    {
        get
        {
            return BindedUser.Avatar;
        }
    }

    private ImageSource? image;

    public ImageSource? Image
    {
        get => image;
        set
        {
            image = value;
            OnPropertyChanged(nameof(Image));
        }
    }

    private string textContent;
    public string? TextContent
    {
        get => textContent;
        set
        {
            textContent = value;
            OnPropertyChanged(nameof(TextContent));
        }
    }

    public uint MessageId { get; init; }

    private string Path { get; set; }

    public string FactoredTime
    {
        get
        {
            if (Time.Day == DateTime.Now.Day) return $"Today at {Time.Hour}:{Time.Minute}";
            else if (Time.Day == DateTime.Now.Day - 1) return $"Yesterday at {Time.Hour}:{Time.Minute}";
            else return $"{Time.Day}/{Time.Month}/{Time.Year} at {Time.Hour}:{Time.Minute}";
        }
    }

    public ChatMessage(User user, string message)
    {
        TextContent = message;
        BindedUser = user;
        Time = DateTime.Now;

        IsText = true;
        IsImage = false;
    }

    public ChatMessage(string message)
    {
        TextContent = message;
        BindedUser = LocalUser.UserRef;
        Time = DateTime.Now;

        IsText = true;
        IsImage = false;
    }

    public ChatMessage(ImageSource image)
    {
        Image = image;
        BindedUser = LocalUser.UserRef;
        Time = DateTime.Now;

        IsText = false;
        IsImage = true;
    }

    public ChatMessage(ChatMessageCacheModel cachedMessage)
    {
        BindedUser = User.GetUser(cachedMessage.UserId);

        if (cachedMessage.IsText)
        {
            IsText = true;
            IsImage = false;

            Time = new DateTime((long)cachedMessage.Date);
            TextContent = cachedMessage.Message;
        }
    }

    public ChatMessage(MessagePacket packet, bool checkCache = true, bool isNewMessage = false, bool isCacheAble = false)
    {
        BindedUser = User.CreateOrGet(packet.Username,packet.UserId);

        if (packet.MessageType == "text") 
        {
            TextContent = packet.ContentString;
            IsImage = false;
            IsText = true;
        }
        else
        {
            IsImage = true;
            IsText = false;


            //Check cache only if getting messages when entering to chat
            if (checkCache)
            {
                Path = packet.ContentString.Substring(9); // 9 to remove server folder path
                byte[] CacheBuffer = Cache.ReadFileBytesCache(Path);

                if (CacheBuffer is null || CacheBuffer.Length == 0)
                {
                    Logger.Push("Cache buffer is null", LogLevel.Warning);
                    Debug.Write($"Deserializing message {packet.MessageType} {Encoding.UTF8.GetString(packet.Content)}");

                    ChatImagePacket imagePacket = new ChatImagePacket(packet.ContentString, packet.MessageType);
                    SocketCore.SendCallback(imagePacket, Token.CHAT_IMAGE_REQUEST, GetImage);
                    ImageRequestQueue.AddRequest(imagePacket, MessageId, GetImage);
                    return;
                }

                ImageSource src = ImageSource.FromStream(() => new MemoryStream(CacheBuffer));
                MainThread.BeginInvokeOnMainThread(() => { Image = src; });
            }
            else if(!checkCache && !isNewMessage)
            {
                ChatImagePacket imagePacket = new ChatImagePacket(packet.ContentString, packet.MessageType);
                SocketCore.SendCallback(imagePacket, Token.CHAT_IMAGE_REQUEST, GetImage);
                ImageRequestQueue.AddRequest(imagePacket, MessageId, GetImage);
            }
            else
            {
                ImageSource imageSrc =
                    ImageSource.FromStream(() => new MemoryStream(packet.Content));
                MainThread.BeginInvokeOnMainThread(() => Image = imageSrc);
            }
        }

        Time = packet.Time;
    }

    public void GetImage(object packet)
    {
        ImageRequestQueue.RemoveRequest(MessageId);

        byte[] buffer = Essential.GetImageBuffer((string)packet);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Image = ImageSource.FromStream(() => new MemoryStream(buffer));
        });

        Cache.SaveToCache(buffer, Path);
    }
}