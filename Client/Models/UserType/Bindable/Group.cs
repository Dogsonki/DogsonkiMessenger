using Client.Networking.Core;
using Client.Pages;
using System.ComponentModel;

namespace Client.Models.UserType.Bindable;

[Bindable(BindableSupport.Yes)]
public class Group : BindableObject
{
    public static List<Group> Groups = new List<Group>();

    public string Name { get; set; }
    public int Id { get; set; }

    public Command OpenChatCommand { get; set; }

    public Group(string groupName, int groupId)
    {
        Name = groupName;
        Id = groupId;
        OpenChatCommand = new Command(OpenChat);
    }

    public static Group CreateOrGet(string name, int id)
    {
        Group group;
        if ((group = Groups.Find(x => x.Id == id)) != null)
            return group;

        return new Group(name, id);
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