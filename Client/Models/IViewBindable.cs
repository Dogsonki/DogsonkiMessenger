using System.ComponentModel;

namespace Client.Models;

public interface IViewBindable
{
    public IViewBindable View { get; }
    public BindableType BindType { get; } 
    public string Name { get; }
    public uint Id { get; }
    public string AvatarPath { get; }
    public string AvatarImageSource { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public static IViewBindable CreateOrGet(string name, uint id, bool isGroup)
    {
        if(isGroup)
        {
            return Group.CreateOrGet(name, id);
        }
        else
        {
            return User.CreateOrGet(name, id);
        }
    }

    public static IViewBindable Get(uint id, bool isGroup)
    {
        if (isGroup)
        {
            return Group.GetGroup(id);
        }
        else
        {
            return User.GetUser(id);
        }
    }
}

public enum BindableType
{
    LocalUser,
    User,
    Group,
    Any
}
