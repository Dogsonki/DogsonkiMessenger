using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;
using Newtonsoft.Json;
using Client.IO;

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

            byte[] buffer = Essential.GetImageBuffer(img.ImageData);

            Cache.SaveToCache(buffer, "user_avatar" + img.UserId);

            AvatarCacheStorage.SaveAvatarCache(DateTime.Now.Ticks, img.UserId);
            user.SetAvatar(buffer);
        }
        catch(Exception ex)
        {
            Debug.Error(ex);
        }
    }
}
