namespace Client.Models.Bindable;

public partial class GroupUser
{
    public bool IsAdmin { get; set; }
    public User UserRef;

    public GroupUser(bool isAdmin, User user)
    {
        IsAdmin = isAdmin;
        UserRef = user;
    }
}
