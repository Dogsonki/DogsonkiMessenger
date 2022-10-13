using Client.Networking.Core;
using System.ComponentModel;
using Client.Utility;
using Client.Networking.Packets;
using Client.IO.Cache;
using Client.IO.Cache.Models;

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
                byte[] CacheBuffer = Cache.ReadCache(Path);

                if (CacheBuffer is null || CacheBuffer.Length == 0)
                {
                    Logger.Push("Cache buffer is null", TraceType.Func, LogLevel.Warning);

                    ChatImagePacket imagePacket = new ChatImagePacket(packet.ContentString, packet.MessageType);
                    SocketCore.SendCallback(GetImage, imagePacket, Token.CHAT_IMAGE_REQUEST);
                    ImageRequestQueue.AddRequest(imagePacket, this);
                    return;
                }

                ImageSource src = ImageSource.FromStream(() => new MemoryStream(CacheBuffer));
                MainThread.BeginInvokeOnMainThread(() => { Image = src; });
            }
            else if(!checkCache && !isNewMessage)
            {
                ChatImagePacket imagePacket = new ChatImagePacket(packet.ContentString, packet.MessageType);
                SocketCore.SendCallback(GetImage, imagePacket, Token.CHAT_IMAGE_REQUEST);
                ImageRequestQueue.AddRequest(imagePacket, this);
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
        ImageRequestQueue.RemoveRequest(this);

        string bufferString = (string)packet;
        byte[] buffer;

        ImageSource src = UserImageRequestPacket.GetImageSource(out buffer, bufferString);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Image = src;
        });

        Cache.SaveToCache(buffer, Path);
    }
}