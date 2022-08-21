using Client.IO;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;
using Newtonsoft.Json;

namespace Client.Models;

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
            UserImageRequestPacket img = packet.ModelCast<UserImageRequestPacket>();

            if (string.IsNullOrEmpty(img.ImageData) || img.ImageData == " ")
            {
                return;
            }

            var user = User.GetUser(img.UserId);

            if (user is null)
            {
                return;
            }

            string avat = img.ImageData.Substring(2);
            avat = avat.Substring(0, avat.Length - 1);

            byte[] imgBuffer = Convert.FromBase64String(avat);
            Cache.SaveToCache(imgBuffer, "avatar" + img.UserId);
            MainThread.BeginInvokeOnMainThread(() => user.Avatar = ImageSource.FromStream(() => new MemoryStream(imgBuffer)));
        }
        catch(Exception ex)
        {
            SocketCore.Send(ex);
            Debug.Error(ex);
        }
    }
}
