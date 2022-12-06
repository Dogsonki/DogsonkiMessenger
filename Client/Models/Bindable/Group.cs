using Client.Networking.Core;
using System.ComponentModel;
using Client.IO;
using Client.Networking.Packets;
using Newtonsoft.Json;
using Client.Utility;

namespace Client.Models.Bindable;

[Bindable(BindableSupport.Yes)]
public class Group : BindableObject, IViewBindable
{
    public BindableType BindType { get; set; }

    public static List<Group> Groups = new List<Group>();

    public List<GroupUser> Users = new List<GroupUser>();

    public string Name { get; set; }
    public uint Id { get; set; }

    private ImageSource avatar;

    public ImageSource Avatar
    {
        get
        {
            return avatar;  
        }
        set
        {
            Debug.Write("Avatar setted ?"+value.IsEmpty);
            avatar = value;
            OnPropertyChanged(nameof(Avatar));
        }
    }

    public Group(string groupName, uint groupId)
    {
        Name = groupName;
        Id = groupId;

        if (!AvatarManager.SetAvatar(this))
        {
            SocketCore.SendCallback(groupId, Token.GROUP_AVATAR_REQUEST, (_) =>
            {
                GroupImageRequestPacket? image = JsonConvert.DeserializeObject<GroupImageRequestPacket>((string)_);
                if (image is null) return;

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

    public static Group CreateOrGet(string name, uint id)
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