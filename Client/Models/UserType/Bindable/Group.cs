using Client.Networking.Core;
using Client.Pages;
using System.ComponentModel;

namespace Client.Models.UserType.Bindable;

[Bindable(BindableSupport.Yes)]
public class Group : BindableObject
{
    public static List<Group> Groups = new List<Group>();

    public List<GroupUser> Users = new List<GroupUser>();

    public string Name { get; set; }
    public int Id { get; set; }

    public Command OpenChatCommand { get; set; }

    public Group(string groupName, int groupId)
    {
        Name = groupName;
        Id = groupId;
        OpenChatCommand = new Command(OpenChat);
    }

    public static Group CreateOrGet(string name, int Id)
    {
        Group group;
        if ((group = Groups.Find(x => x.Id == Id)) != null)
            return group;

        return new Group(name, Id);
    }

    public static Group? Get(int Id)
    {
        return Groups.Find(x => x.Id == Id);
    }

    public void AddUser(GroupUser groupUser)
    {
        Debug.Write($"Adding user: {groupUser.IsAdmin} {groupUser.UserRef.Username} {groupUser.UserRef.UserId}");
        Users.Add(groupUser);
    }

    private void OpenChat()
    {
        SocketCore.Send($"{Id}", Token.GROUP_CHAT_INIT);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StaticNavigator.Push(new MessagePage(this));
        });
    }

}