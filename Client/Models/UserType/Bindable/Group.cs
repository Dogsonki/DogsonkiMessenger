namespace Client.Models.UserType.Bindable;

public class Group
{
    public string GroupName { get; set; }
    public int GroupId { get; set; }

    public Group(string groupName, int groupId)
    {
        GroupName = groupName;
        GroupId = groupId;
    }
}