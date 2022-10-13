using Client.Networking.Core;
using System.ComponentModel;
using Client.IO.Cache;

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
            OnPropertyChanged(nameof(avatar));
        }
    }

    public Group(string groupName, int groupId)
    {
        Name = groupName;
        Id = groupId;

        byte[] avatarCacheBuffer = Cache.ReadCache("group_avatar" + Id);

        if (avatarCacheBuffer is not null)
        {
            ImageSource src = ImageSource.FromStream(() => new MemoryStream(avatarCacheBuffer));
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Avatar = src;
            });
        }
        else
        {
            SocketCore.Send(Id, Token.GROUP_AVATAR_REQUEST);
        }
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