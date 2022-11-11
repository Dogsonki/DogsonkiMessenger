using System.Collections.Specialized;
using System.ComponentModel;

namespace Client.Models.Bindable;

/// <summary>
/// Object used to be used as BindableObject in ObservableCollection T with onPropertyChanged
/// </summary>
public class AnyListBindable : BindableObject
{
    public User? BindedUser { get; set; }
    public Group? BindedGroup { get; set; }
    private readonly bool UseUserInput;
    private readonly bool UseGroupInput;

    private readonly BindType type;

    public string Username
    {
        get
        {
            if(type == BindType.Group)
            {
                return BindedGroup.Name;
            }

            return BindedUser.Username;
        }
    }
    public int Id 
    {
        get
        {
            if (type == BindType.Group)
            {
                return BindedGroup.Id;
            }

            return BindedUser.UserId;
        }
     }

    private Command? input;
    public Command? Input
    {
        get
        {
            return input;
        }
        set
        {
            input = value;
        }
    }

    public ImageSource BindedAvatar
    {
        get
        {
            if(type == BindType.Group)
            {
                return BindedGroup.Avatar;
            }
            return BindedUser.Avatar;
        }
    }

    public bool IsGroup => BindedGroup is not null;

    public AnyListBindable(User user, Command input = null)
    {
        type = BindType.User;
        BindedUser = user;

        Input = input;
    }

    public AnyListBindable(Group group, Command input = null)
    {
        type = BindType.Group;
        BindedGroup = group;

        Input = input;
    }
}

enum BindType
{
    User,
    Group
}