using System.Text;
using Client.Models;
using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Utility;
using Client.Networking.Packets;
using Newtonsoft.Json;

namespace Client.IO;

public static class AvatarManager
{
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

            Cache.SaveToCache(buffer, "user_avatar" + img.Id);

            AvatarCacheStorage.SaveAvatarCache(DateTime.Now.Ticks, img.Id);

            user.SetAvatar(buffer);
        });
    }

    private static void SaveAvatarInfo(int time, IViewBindable view)
    {
        Debug.ThrowIfNull(view);

        byte[] info = Encoding.UTF8.GetBytes(time.ToString());

        Cache.SaveToCache(info, GetAvatarInfoPath(view));
    }

    private static int ReadAvatarTime(IViewBindable view)
    {
        string avatarTimePath = GetAvatarInfoPath(view);

        byte[] time = Cache.ReadFileBytesCache(avatarTimePath);

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
        else
        {
            return Cache.ReadFileBytesCache("group_avatar" + view.Id);
        }
    }

    public static bool SetAvatar(IViewBindable view)
    {
        Task.Run(() =>
        {
            byte[]? avatarBuffer = ReadAvatar(view);

            if(avatarBuffer is not null &&  avatarBuffer.Length > 0)
            {
                ImageSource avatarSource = ImageSource.FromStream(() => new MemoryStream(avatarBuffer));
                SetAvatar(view, avatarSource);
            }

            if (view.BindType == BindableType.User || view.BindType == BindableType.LocalUser)
            {
                if(avatarBuffer is null || avatarBuffer.Length > 0)
                {
                    SocketCore.Send(view.Id, Token.USER_AVATAR_REQUEST);
                    return;
                }

                SocketCore.SendCallback(view.Id, Token.GET_USER_AVATAR_TIME, (object avatarTimePacket) =>
                {
                    int time = -1;
                    int newTime = int.Parse((string)avatarTimePacket);

                    if ((time = ReadAvatarTime(view)) == -1)
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
                SocketCore.SendCallback(view.Id, Token.GET_GROUP_AVATAR_TIME, (object avatarTimePacket) =>
                {
                    int time = -1;
                    int newTime = int.Parse((string)avatarTimePacket);

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

        return false;
    }

    public static void SetAvatar(IViewBindable view, byte[] avatarBuffer)
    {
        Task.Run(() =>
        {
            ImageSource source = ImageSource.FromStream(() => new MemoryStream(avatarBuffer));
            SetAvatar(view, source);
        });
    }

    private static void SetAvatar(IViewBindable view, ImageSource avatarSource)
    {
        if(avatarSource == null || avatarSource.IsEmpty)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            view.Avatar = avatarSource;
        });
    }
}