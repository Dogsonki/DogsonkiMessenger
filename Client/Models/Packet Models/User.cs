using Client.IO;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Pages;
using System.ComponentModel;

namespace Client.Models;

[Bindable(BindableSupport.Yes)]
public class User : BindableObject
{
    public static List<User> Users = new List<User>();
    public List<MessageModel> CachedMessages = new List<MessageModel>();

    protected ImageSource avatar;
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
            avatar = value;
            OnPropertyChanged(nameof(Avatar));
        }
    }

    public bool IsLocalUser = false;

    public string Name { get; set; }
    public uint ID { get; set; }

    public Command OpenChatCommand { get; set; }

    public User(string username, uint id,bool isLocalUser = false)
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

        if(AvatarCacheBuffer is not null)
        {
            ImageSource avatar = ImageSource.FromStream(() => new MemoryStream(AvatarCacheBuffer));

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Avatar = avatar;
            });
        }
        else
        {
            Debug.Write("Cached image dose not exists");
        }

        Debug.Write($"SENDING_REQUEST_AVATAR: {id}");
        SocketCore.Send(id, Token.AVATAR_REQUEST);
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

        return new User(username, id,false);
    }

    public static User CreateLocalUser(string username, uint id)
    {
        return new User(username, id, true);
    } 

    public static User GetUser(uint id) => Users.Find(x => x.ID == id);
}
