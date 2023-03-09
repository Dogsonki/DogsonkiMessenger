using System.Text;
using Client.Models;
using Client.Networking.Core;
using Client.Utility;
using Client.Networking.Packets;
using Client.Networking.Models;
using Clinet.IO;

namespace Client.IO;

public static class AvatarManager
{
    /* 1px of png image */
    public const string BlankAvatar = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=";

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

    private static void ManageUserImagePacket(SocketPacket packet)
    {
        Task.Run(() =>
        {
            UserImageRequestPacket? img = packet.Deserialize<UserImageRequestPacket>(); 

            if(img is null) {
                return;
            }

            var user = IViewBindable.Get(img.Id, false);

            if (user is null)
            {
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

    public static string ToJSImageSource(byte[] source) {
        if (source is null) {
            throw new ArgumentNullException("source");
        }

        string base64 = Convert.ToBase64String(source);
        return string.Format("data:image/gif;base64,{0}", base64);
    }

    /// <summary>
    /// Changes avatar of view. If view is type of ViewBindable notifies ui.
    /// </summary>
    /// <param name="view"></param>
    public static void SetAvatar(IViewBindable view)
    {
        view.AvatarImageSource = BlankAvatar;

        Task.Run(() =>
        {
            byte[]? avatarBuffer = ReadAvatar(view);

            if (avatarBuffer is not null && avatarBuffer.Length > 0)
            {
                view.AvatarImageSource = ToJSImageSource(avatarBuffer);
            }

            if (view.BindType == BindableType.User || view.BindType == BindableType.LocalUser)
            {
                if (avatarBuffer is null || avatarBuffer.Length < 0)
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
                            Debug.Write($"Requesting new avatar because of time NewTime:{newTime} TimeFromCache:{time}");
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
        string jsBuffer = ToJSImageSource(avatarBuffer);

        view.AvatarImageSource = jsBuffer;

        if (view.BindType == BindableType.User || view.BindType == BindableType.LocalUser)
        {
            Cache.SaveToCache(avatarBuffer, "user_avatar" + view.Id);
        }
        else
        {
            Cache.SaveToCache(avatarBuffer, "group_avatar" + view.Id);
        }
    }

    public static async Task MediaPickerSet(IViewBindable view)
    {
        byte[] avatarData = await FileManager.FileFromSelectedFile();

        if(avatarData.Length == 0)
        {
            return;
        }

        SetAvatar(view, avatarData);

        if(view.Id == LocalUser.CurrentUser.Id) 
        {
            SocketCore.Send(avatarData, Token.CHANGE_AVATAR);
        }
    }

    public static async Task<byte[]> MediaPickerSet() 
    {
        byte[] avatarData = await FileManager.FileFromSelectedFile();

        return avatarData;
    }
}