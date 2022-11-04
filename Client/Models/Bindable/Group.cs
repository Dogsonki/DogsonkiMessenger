using Client.Networking.Core;
using System.ComponentModel;
using System.Text;
using Client.IO;
using Client.IO.Cache;
using Client.Networking.Packets;
using Client.Utility;
using Newtonsoft.Json;

namespace Client.Models.Bindable;

[Bindable(BindableSupport.Yes)]
public class Group : BindableObject
{
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
            OnPropertyChanged(nameof(Avatar));
        }
    }

    public Group(string groupName, int groupId)
    {
        Debug.Write("Creating avatar group");
        Name = groupName;
        Id = groupId;

        byte[] avatarCacheBuffer = AvatarManager.ReadGroupAvatar(groupId);

     
        if (!AvatarManager.SetGroupAvatar(this, avatarCacheBuffer))
        {
            SocketCore.SendCallback(groupId, Token.GROUP_AVATAR_REQUEST, (_) =>
            {
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