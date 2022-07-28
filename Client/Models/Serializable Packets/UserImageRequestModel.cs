﻿using Client.IO;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
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
    public int UserID { get; set; }

    [JsonConstructor]
    public UserImageRequestModel(string avatar, int login_id)
    {
        ImageData = avatar;
        UserID = login_id;
    }

    public static void ProcessImage(SocketPacket packet)
    {
        try
        {
            UserImageRequestModel img = Essential.ModelCast<UserImageRequestModel>(packet.Data);

            if (string.IsNullOrEmpty(img.ImageData) || img.ImageData == " ")
            {
                return;
            }

            var user = User.GetUser(img.UserID);

            if (user == null)
            {
                Debug.Error("USER_NULL_REFRENCE");
                return;
            }

            string avat = img.ImageData.Substring(2);
            avat = avat.Substring(0, avat.Length - 1);

            byte[] imgBuffer = Convert.FromBase64String(avat);
            Cache.SaveToCache(imgBuffer, "avatar" + img.UserID);
            MainThread.BeginInvokeOnMainThread(() => user.Avatar = ImageSource.FromStream(() => new MemoryStream(imgBuffer)));
        }
        catch(Exception ex)
        {
            SocketCore.Send(ex);
            Debug.Error(ex);
        }
    }
}
