using Client.IO;
using Client.Networking.Core;
using System.ComponentModel;
using Android.Provider;
using Client.Networking.Packets;

namespace Client.Models.UserType.Bindable;

[Bindable(BindableSupport.Yes)]
public class ChatMessage : BindableObject
{
    public User BindedUser { get; init; }
    
    private DateTime Time { get; init; }

    public bool IsImage { get; } = false;

    public bool IsText { get; }

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

    public string textContent;
    public string? TextContent
    {
        get => textContent;
        set
        {
            textContent = value;
            OnPropertyChanged(nameof(TextContent));
        }
    }

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

    public ChatMessage(User user,string message)
    {
        BindedUser = user;
        TextContent = message;
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

    public ChatMessage(MessagePacket packet)
    {
        Debug.Write($"Chat message {packet.MessageType}");
        BindedUser = User.CreateOrGet(packet.Username,packet.UserId);

        if (packet.MessageType == "text") 
        {
            TextContent = packet.ContentString;
            IsImage = false;
            IsText = true;
        }
        else
        {
            Path = packet.ContentString.Substring(9); // 9 to remove server folder path
            byte[] CacheBuffer = Cache.ReadCache(Path);

            if(CacheBuffer is null || CacheBuffer.Length == 0)
            {
                Debug.Write("Requesting image");
                ChatImagePacket imagePacket = new ChatImagePacket(packet.ContentString, packet.MessageType);
                SocketCore.SendCallback(GetImage, imagePacket, Token.CHAT_IMAGE_REQUEST);
                ImageRequestQueue.AddRequest(imagePacket,this);
            }
            else
            {
                ImageSource src = ImageSource.FromStream(() => new MemoryStream(CacheBuffer));
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Image = src;
                });
            }

            IsImage = true;
            IsText = false;
        }

        Time = packet.Time;
    }

    public void GetImage(object packet)
    {
        Debug.Write("Getting image, and removing it from queue");
        ImageRequestQueue.RemoveRequest(this);

        string bufferString = (string)packet;
        byte[] buffer;

        ImageSource src = UserImageRequestPacket.GetImageSource(out buffer, bufferString);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Debug.Write("GEt image> >>> ");
            Image = src;
        });

        Cache.SaveToCache(buffer, Path);
    }
}