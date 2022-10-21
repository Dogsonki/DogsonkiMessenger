﻿using Client.IO.Cache;
using Client.Networking.Core;
using System.ComponentModel;
using Client.Utility;

namespace Client.Models.Bindable;

[Bindable(BindableSupport.Yes)]
public class User : BindableObject
{
    /*WIP*/
    private string? tag;
    public string Tag
    {
        get { return "System"; }
        set { tag = value; }
    }

    public bool isBot { get; set; }
    public bool VisibleTag { get; set; } = false;

    public static List<User> Users = new List<User>();

    private ImageSource avatar;
    public ImageSource Avatar
    {
        get
        {
            if (IsLocalUser)
            {
                return LocalUser.Current.Avatar;
            }
            return avatar;
        }
        set
        {
            if (IsLocalUser)
            {
                LocalUser.Current.Avatar = value;
            }
            else
            {
                avatar = value;
                OnPropertyChanged(nameof(avatar));
            }
        }
    }

    private int dogeCoins;
    public int DogeCoins
    {
        get
        {
            return dogeCoins;
        }
        set
        {
            dogeCoins = value;
            OnPropertyChanged(nameof(DogeCoins));
        }
    }

    /* As LocalUser have his own binding but User reuse it LocalUser have to be binding as LocalUser.<BindableObject> */
    public bool IsLocalUser = false;

    /* Username and ID will never change then don't make them as OnPropertyChanged */
    public string Username { get; set; }
    public int UserId { get; set; }

    public User(string username, int id, bool isLocalUser = false, UserCreateFlags flags = UserCreateFlags.Default)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Users.Find(x => x.UserId == id) != null)
                return;

            Username = username;
            UserId = id;
            IsLocalUser = isLocalUser;

            Users.Add(this);
        });

        if (flags.HasFlag(UserCreateFlags.UseDefaultAvatar) && !flags.HasFlag(UserCreateFlags.Default) && !flags.HasFlag(UserCreateFlags.IsSystemUser))
        {
            byte[] avatarCacheBuffer = Cache.ReadCache("user_avatar_default");
            SetAvatar(avatarCacheBuffer);
        }
        else if (flags.HasFlag(UserCreateFlags.IsSystemUser) && !flags.HasFlag(UserCreateFlags.Default))
        {
            /*TODO: Set System Bot Avatar */
        }
        else 
        {
            byte[] avatarCacheBuffer = Cache.ReadCache("user_avatar" + id);

            if (!SetAvatar(avatarCacheBuffer))
            {
                Debug.Write($"Cache avatar dose not exists {Username}:{UserId}");
                SocketCore.Send(id, Token.USER_AVATAR_REQUEST);
            }
        }

    }

    /// <summary>
    /// Sets avatar to user
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns>Returns false if buffer is null or empty</returns>
    public bool SetAvatar(byte[] buffer)
    {
        if (buffer is not null && buffer.Length > 0)
        {
            Cache.RemoveFromCache($"user_avatar{UserId}");
            ImageSource src = ImageSource.FromStream(() => new MemoryStream(buffer));
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Avatar = src;
            });
            Cache.SaveToCache(buffer,$"user_avatar{UserId}");

            Logger.Push($"User avatar set {UserId}",TraceType.Func,LogLevel.Debug);

            return true;
        }

        return false;
    }

    public static void ClearUsers() => Users.Clear();

    public static User CreateOrGet(string username, int id, UserCreateFlags flags = UserCreateFlags.Default)
    {
        User? user;
        if ((user = Users.Find(x => x.UserId == id)) != null)
            return user;

        return new User(username, id, false);
    }

    public static User CreateLocalUser(string username, int id)
    {
        CreateSystemBot();
        return new User(username, id, true);
    }

    /// <summary>
    /// User "bot" that is visible only for client, sends messages with errors and warnings to client
    /// </summary>
    public static User CreateSystemBot()
    {
        User? _;
        if ((_ = GetUser(-100)) is not null) return _;

        User systemBot = new User("System", -100, flags: UserCreateFlags.IsSystemUser)
        {
            isBot = true,
            Tag = "System",
            VisibleTag = true
        };
        return systemBot;
    }

    public static User GetSystemBot()
    {
        User? systemBot = GetUser(-100);
        return systemBot is not null ? systemBot : CreateSystemBot();
    }

    public static User? GetUser(int id) => Users.Find(x => x.UserId == id);
}

[Flags]
public enum UserCreateFlags
{
    UseDefaultAvatar,
    IsSystemUser,
    Default
}