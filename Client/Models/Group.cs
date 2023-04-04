using Client.IO;

namespace Client.Models;

public class Group : ViewBindable
{
    public readonly static List<Group> Groups = new List<Group>();

    public readonly List<User> Members = new List<User>();

    public Group(string name, uint id, bool loadAvatar = true) : base(BindableType.Group, name, id)
    {
        if (loadAvatar)
        {
            AvatarManager.SetAvatar(this);
        }

        Groups.Add(this);
    }

    public static Group CreateOrGet(string name, uint id, bool loadAvatar)
    {
        Group group;
        if ((group = Groups.Find(x => x.Id == id)) != null)
            return group;

        return new Group(name, id, loadAvatar);
    }

    public static Group? GetGroup(uint id)
    {
        return Groups.Find(x => x.Id == id);
    }

    public void AddUser(User groupUser)
    {
        Members.Add(groupUser);
    }
}