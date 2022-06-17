using Client.Networking.Core;
using Client.Networking.Model;
using Client.Pages;

namespace Client.Models;

public class PersonFoundModel
{
    public UserModel User { get; set; }
    public Command OpenChatCommand { get; }
    public ImageSource Image { get; set; }

    public PersonFoundModel(UserModel user)
    {
        OpenChatCommand = new Command(OpenChat);
        User = user;
    }

    //Don't push messages as ModalPages, keyboard is bugged af
    private void OpenChat()
    {
        SocketCore.Send($"{User.Name}", Token.INIT_CHAT);
        StaticNavigator.Push(new MessagePage(User));
    }
}