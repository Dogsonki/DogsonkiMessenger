using System.Text;
using Client.Models.Bindable;
using Client.Networking.Core;

namespace Client.IO;

public static class AvatarManager
{
    public static byte[] ReadUserAvatar(int userId)
    {
        byte[] avatarCacheBuffer = Cache.ReadFileBytesCache("user_avatar" + userId);

        if (avatarCacheBuffer is null || avatarCacheBuffer.Length == 0)
        {
            Debug.Write($"User Id: {userId} has null avatar");
        }

        return avatarCacheBuffer;
    }

    public static byte[] ReadGroupAvatar(int groupId)
    {
        byte[] avatarCacheBuffer = Cache.ReadFileBytesCache("group_avatar" + groupId);

        if (avatarCacheBuffer is null || avatarCacheBuffer.Length == 0)
        {
            Debug.Write($"User Id: {groupId} has null avatar");
        }

        return avatarCacheBuffer;
    }

    private static int ReadUserAvatarInfo(int userId)
    {
        byte[] time = Cache.ReadFileBytesCache("user_avatar_info" + userId);

        if (time is null || time.Length == 0)
        {
            return -1;
        }

        return int.Parse(Encoding.UTF8.GetString(time));
    }

    private static int ReadGroupAvatarInfo(int groupId)
    {
        byte[] time = Cache.ReadFileBytesCache("user_avatar_info" + groupId);

        if (time is null || time.Length == 0)
        {
            return -1;
        }

        return int.Parse(Encoding.UTF8.GetString(time));
    }

    private static bool SaveUserAvatarInfo(int time, int userId)
    {
        byte[] info = Encoding.ASCII.GetBytes(time.ToString());
        Cache.SaveToCache(info, "user_avatar_info" + userId);
        return true;
    }

    private static bool SaveGroupAvatarInfo(int time, int groupId)
    {
        byte[] info = Encoding.ASCII.GetBytes(time.ToString());
        Cache.SaveToCache(info, "group_avatar_info" + groupId);
        return true;
    }

    public static bool SetUserAvatar(User user, byte[] avatar)
    {
        if (avatar is null || avatar.Length == 0)
        {
            Debug.Error("Cannot set null avatar");
            return false;
        }

        SocketCore.SendCallback(user.UserId, Token.GET_USER_AVATAR_TIME, (object _) =>
        {
            int time = -1;
            int newTime = int.Parse((string)_);

            if ((time = ReadUserAvatarInfo(user.UserId)) == -1)
            {
                SaveUserAvatarInfo(newTime, user.UserId);
                
                user.SetAvatar(avatar);
            }
            else
            {
                if (newTime > time)
                {
                    Debug.Write($"Time {newTime} > {time}");
                    SocketCore.Send(user.UserId, Token.USER_AVATAR_REQUEST);
                }
                else
                {
                    user.SetAvatar(avatar);
                }
            }
        });

        return true;
    }

    public static bool SetGroupAvatar(Group group, byte[] avatar)
    {
        if (avatar is null || avatar.Length == 0)
        {
            Debug.Error("Cannot set null avatar");
            return false;
        }

        SocketCore.SendCallback(group.Id, Token.GET_GROUP_AVATAR_TIME, (object _) =>
        {
            int time = -1;
            int newTime = int.Parse((string)_);

            if ((time = ReadGroupAvatarInfo(group.Id)) == -1)
            {
                SaveGroupAvatarInfo(newTime, group.Id);

                group.SetAvatar(avatar);
            }
            else
            {
                if (newTime > time)
                {
                    SocketCore.Send(group.Id, Token.GROUP_AVATAR_REQUEST);
                }
                else
                {
                    group.SetAvatar(avatar);
                }
            }
        });

        return true;
    }
}