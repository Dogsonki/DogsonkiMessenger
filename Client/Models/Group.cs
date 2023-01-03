using Client.IO;
using System.ComponentModel;

namespace Client.Models;

public class Group : IViewBindable
{
    public readonly static List<Group> Groups = new List<Group>();
    public event PropertyChangedEventHandler? PropertyChanged;

    public BindableType BindType { get; set; }

    public readonly List<User> Users = new List<User>();

    public string Name { get; set; }
    public uint Id { get; }

    public IViewBindable View => this;

    public string AvatarPath => $"group_avatar{Id}";

    public string? AvatarImageSource { get; set; }

    public Group(string groupName, uint groupId)
    {
        Name = groupName;
        Id = groupId;

        AvatarManager.SetAvatar(this);

        Groups.Add(this);
    }

    public static Group CreateOrGet(string name, uint id)
    {
        Group group;
        if ((group = Groups.Find(x => x.Id == id)) != null)
            return group;

        return new Group(name, id);
    }

    public static Group? GetGroup(uint id)
    {
        return Groups.Find(x => x.Id == id);
    }

    public void AddUser(User groupUser)
    {
        Users.Add(groupUser);
    }
}