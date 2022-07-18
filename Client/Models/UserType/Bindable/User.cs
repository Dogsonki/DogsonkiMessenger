using Client.IO;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Pages;
using System.ComponentModel;

namespace Client.Models.UserType.Bindable;

[Bindable(BindableSupport.Yes)]
public class User : BindableObject
{
    public static List<User> Users = new List<User>();
    public List<MessageModel> CachedMessages = new List<MessageModel>();

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
                OnPropertyChanged(nameof(Avatar));
            }
        }
    }

    /* As LocalUser have his own binding but User reuse it LocalUser have to be binding as LocalUser.<BindableObject> */
    public bool IsLocalUser = false;

    /* Name and ID will never change then don't make them as OnPropertyChanged */
    public string Name { get; set; }
    public uint ID { get; set; }

    public Command OpenChatCommand { get; set; }

    public User(string username, uint id, bool isLocalUser = false)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Users.Find(x => x.ID == id) != null)
                return;

            Name = username;
            ID = id;
            IsLocalUser = isLocalUser;
            OpenChatCommand = new Command(OpenChat);

            Users.Add(this);
        });

        byte[] AvatarCacheBuffer = Cache.ReadCache("avatar" + id);

        if (AvatarCacheBuffer.Length > 0)
        {
            ImageSource src = ImageSource.FromStream(() => new MemoryStream(AvatarCacheBuffer));
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Avatar = src;
            });
        }
        else
        {
            SocketCore.Send(id, Token.AVATAR_REQUEST);
        }
    }

    public static void ClearUsers() => Users.Clear();

    private void OpenChat()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SocketCore.Send($"{Name}", Token.INIT_CHAT);
            StaticNavigator.Push(new MessagePage(this));
        });
    }

    public static User CreateOrGet(string username, uint id)
    {
        User user;
        if ((user = Users.Find(x => x.ID == id)) != null)
            return user;

        return new User(username, id, false);
    }

    public static User CreateLocalUser(string username, uint id)
    {
        return new User(username, id, true);
    }

    public static User GetUser(uint id) => Users.Find(x => x.ID == id);
}
