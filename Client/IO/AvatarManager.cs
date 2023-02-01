using System.Text;
using Client.Models;
using Client.Networking.Core;
using Client.Utility;
using Client.Networking.Packets;
using Newtonsoft.Json;
using Client.Networking.Models;
using Clinet.IO;

namespace Client.IO;

public static class AvatarManager
{
    public const string BlankAvatar = "data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=";

    static AvatarManager()
    {
        SocketCore.SetGlobalOnToken(Token.USER_AVATAR_REQUEST, ManageUserImagePacket);
    }

    private static string GetAvatarInfoPath(IViewBindable view)
    {
        if (view.BindType == BindableType.User || view.BindType == BindableType.LocalUser)
           return "user_avatar_info" + view.Id;
        else
           return "group_avatar_info" + view.Id;
    }

    private static void ManageUserImagePacket(object packet)
    {
        Task.Run(() =>
        {
            UserImageRequestPacket? img = JsonConvert.DeserializeObject<UserImageRequestPacket>(Convert.ToString(packet));

            Debug.ThrowIfNull(img);

            var user = User.GetUser(img.Id);

            if (user is null)
            {
                SocketCore.Send("user image null");
                return;
            }

            byte[] buffer = Essential.GetImageBuffer(img.ImageData);

            if(buffer.Length == 0)
            {
                Logger.Push($"Downloaded avatar {img.Id} was empty", LogLevel.Error);
                return;
            }

            Cache.SaveToCache(buffer, "user_avatar" + img.Id);

            SaveAvatarInfo((int)DateTime.Now.Ticks, user);

            SetAvatar(user, buffer);
        });
    }

    public static void SaveAvatarInfo(int time, IViewBindable view)
    {
        Debug.ThrowIfNull(view);

        byte[] info = Encoding.UTF8.GetBytes(time.ToString());

        Cache.SaveToCache(info, GetAvatarInfoPath(view));
    }

    private static int ReadAvatarTime(IViewBindable view)
    {
        string avatarTimePath = GetAvatarInfoPath(view);

        byte[]? time = Cache.ReadFileBytesCache(avatarTimePath);

        if (time is null || time.Length == 0)
        {
            return -1;
        }

        return int.Parse(Encoding.UTF8.GetString(time));
    }

    private static byte[]? ReadAvatar(IViewBindable view)
    {
        if(view.BindType == BindableType.User || view.BindType == BindableType.LocalUser)
        {
            return Cache.ReadFileBytesCache("user_avatar" + view.Id);
        }
        return Cache.ReadFileBytesCache("group_avatar" + view.Id);
    }

    public static void SetAvatar(IViewBindable view)
    {
        Task.Run(() =>
        {
            byte[]? avatarBuffer = ReadAvatar(view);
            if(avatarBuffer is not null &&  avatarBuffer.Length > 0)
            {
                view.AvatarImageSource = FileManager.ToJSImageSource(avatarBuffer);
            }
            else
            {
                view.AvatarImageSource = BlankAvatar;
            }

            if (view.BindType == BindableType.User || view.BindType == BindableType.LocalUser)
            {
                if(avatarBuffer is null || avatarBuffer.Length < 0)
                {
                    SocketCore.Send(view.Id, Token.USER_AVATAR_REQUEST);
                    return;
                }

                SocketCore.SendCallback(view.Id, Token.GET_USER_AVATAR_TIME, (SocketPacket packet) =>
                {
                    int time = ReadAvatarTime(view);
                    int newTime = packet.ToInt();
                    
                    if (time == -1)
                    {
                        SaveAvatarInfo(newTime, view);
                    }
                    else
                    {
                        if (newTime > time)
                        {
                            SocketCore.Send(view.Id, Token.USER_AVATAR_REQUEST);
                        }
                    }
                });
            }
            else
            {
                SocketCore.SendCallback(view.Id, Token.GET_GROUP_AVATAR_TIME, (SocketPacket packet) =>
                {
                    int time = -1;
                    int newTime = packet.ToInt();

                    if ((time = ReadAvatarTime(view)) == -1)
                    {
                        SaveAvatarInfo(newTime, view);
                    }
                    else
                    {
                        if (newTime > time)
                        {
                            SocketCore.Send(view.Id, Token.GROUP_AVATAR_REQUEST);
                        }
                    }
                });

            }
        });
    }

    /// <summary>
    /// Decodes image buffer to JSImage and saves it to cache, dose not sends it to server
    /// </summary>
    public static void SetAvatar(IViewBindable view, byte[] avatarBuffer)
    {
        string jsBuffer = FileManager.ToJSImageSource(avatarBuffer);
        view.AvatarImageSource = jsBuffer;

        if(view.BindType == BindableType.User || view.BindType == BindableType.LocalUser)
        {
            Cache.SaveToCache(avatarBuffer, "user_avatar" + view.Id);
        }
    }

    public static async Task MediaPickerSet()
    {
        byte[] avatarData = await FileManager.FileFromSelectedFile();

        SetAvatar(LocalUser.CurrentUser, avatarData);

        SocketCore.Send(avatarData, Token.CHANGE_AVATAR);
    }
}