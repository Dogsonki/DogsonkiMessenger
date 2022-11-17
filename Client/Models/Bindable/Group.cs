using Client.Networking.Core;
using System.ComponentModel;
using Client.IO;
using Client.Networking.Packets;
using Newtonsoft.Json;

namespace Client.Models.Bindable;

[Bindable(BindableSupport.Yes)]
public class Group : IBindableType, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public BindableType Type { get; set; }

    public static List<Group> Groups = new List<Group>();

    public List<GroupUser> Users = new List<GroupUser>();

    public string Name { get; set; }
    public int Id { get; set; }

    private ImageSource avatar;

    public ImageSource Avatar
    {
        get
        {
            return avatar;  
        }
        set
        {
            avatar = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Avatar)));
        }
    }

    public Group(string groupName, int groupId)
    {
        Name = groupName;
        Id = groupId;

        byte[] avatarCacheBuffer = AvatarManager.ReadGroupAvatar(groupId);

     
        if (!AvatarManager.SetGroupAvatar(this, avatarCacheBuffer))
        {
            SocketCore.SendCallback(groupId, Token.GROUP_AVATAR_REQUEST, (_) =>
            {
                return;
                GroupImageRequestPacket? image = JsonConvert.DeserializeObject<GroupImageRequestPacket>((string)_);
                if (image is null) return;

                LocalUser.UserRef.SetAvatar(image.ImageData);

                Avatar = LocalUser.avatar;

                SetAvatar(image.ImageData);
            });
        }

        Groups.Add(this);
    }

    public void SetAvatar(byte[] buffer)
    {
        ImageSource src = ImageSource.FromStream(() => new MemoryStream(buffer));

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Avatar = src;
        });
    }

    public static Group CreateOrGet(string name, int id)
    {
        Group group;
        if ((group = Groups.Find(x => x.Id == id)) != null)
            return group;

        return new Group(name, id);
    }

    public static Group? GetGroup(int id)
    {
        return Groups.Find(x => x.Id == id);
    }

    public void AddUser(GroupUser groupUser)
    {
        Users.Add(groupUser);
    }
}