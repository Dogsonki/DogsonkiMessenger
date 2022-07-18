using Client.Models.UserType.Bindable;
using Client.Networking.Model;
using Client.Utility;
using Newtonsoft.Json;

namespace Client.Models;

[Serializable]
public class UserImageRequestModel
{
    [JsonProperty("avatar")]
    public string ImageData { get; set; }
    [JsonProperty("login_id")]
    public uint UserID { get; set; }

    [JsonConstructor]
    public UserImageRequestModel(string avatar, uint login_id)
    {
        ImageData = avatar;
        UserID = login_id;
    }

    public static void ProcessImage(SocketPacket packet)
    {
        UserImageRequestModel img = Essential.ModelCast<UserImageRequestModel>(packet.Data);

        if (img.ImageData == " ")
        {
            return;
        }

        var user = User.GetUser(img.UserID);

        if (user == null)
        {
            Debug.Error("USER_AVATAR_NULL_REFRENCE");
            return;
        }

        string avat = img.ImageData.Substring(2);
        avat = avat.Substring(0, avat.Length - 1);

        byte[] imgBuffer = Convert.FromBase64String(avat);

        MainThread.BeginInvokeOnMainThread(() => user.Avatar = ImageSource.FromStream(() => new MemoryStream(imgBuffer)));
    }
}
