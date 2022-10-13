using Client.IO.Cache;
using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
public class UserImageRequestPacket
{
    [JsonProperty("avatar")]
    public string ImageData { get; set; }
    [JsonProperty("login_id")]
    public int UserId { get; set; }

    [JsonConstructor]
    public UserImageRequestPacket(string avatar, int login_id)
    {
        ImageData = avatar;
        UserId = login_id;
    }

    public static void ProcessImage(SocketPacket packet)
    {
        SocketCore.Send("process");
        try
        {
            UserImageRequestPacket? img = packet.ModelCast<UserImageRequestPacket>();

            if (img is null || string.IsNullOrEmpty(img.ImageData) || img.ImageData == " ")
            {
                return;
            }

            var user = User.GetUser(img.UserId);

            if (user is null)
            {
                SocketCore.Send("user image null");
                return;
            }
            SocketCore.Send("buffer");
            byte[] buffer;
            ImageSource imgS = GetImageSource(out buffer, img.ImageData);

            Cache.SaveToCache(buffer, "user_avatar" + img.UserId);

            user.SetAvatar(buffer);
        }
        catch(Exception ex)
        {
            Debug.Error(ex);
        }
    }

    public static ImageSource GetImageSource(out byte[] buffer, string imageBuffer)
    {
        string avat = imageBuffer.Substring(2);
        avat = avat.Substring(0, avat.Length - 1);
        byte[] imgBuffer = Convert.FromBase64String(avat);
        buffer = imgBuffer;
        return ImageSource.FromStream(() => new MemoryStream(imgBuffer));
    }
}
