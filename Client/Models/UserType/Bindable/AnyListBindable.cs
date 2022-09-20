namespace Client.Models.UserType.Bindable;

/// <summary>
/// Object used to be used as BindableObject in ObservableCollection T with onPropertyChanged
/// </summary>
public class AnyListBindable : BindableObject
{
    private readonly User? BindedUser;
    private readonly Group? BindedGroup;
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

    public ImageSource Avatar
    {
        get
        {
            if(type == BindType.Group)
            {
                return null;
            }
            return BindedUser.Avatar;
        }
    }

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