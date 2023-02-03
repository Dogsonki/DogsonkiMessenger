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
            if (LocalUser.CurrentUser.Id == id)
            {
                return LocalUser.CurrentUser;
            }

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
            if(LocalUser.CurrentUser.Id == id)
            {
                return LocalUser.CurrentUser;
            }
            return User.GetUser(id);
        }
    }

    public static IViewBindable CreateTestView(string name, uint id, bool isGroup)
    {
#if RELEASE
        throw new Exception("Test view was created in release mode");
#endif
        if (!isGroup)
        {
            return new User(name, id, setAvatar: false);
        }
        else
        {
            return new Group(name, id, setAvatar: false);
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
