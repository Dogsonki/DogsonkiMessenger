using Client.Networking;
using Client.Utility;
using Xamarin.Forms;

namespace Client.Models
{
    public class PeronFoundModel
    {
        public string Username { get; set; }
        public Command OpenChatCommand { get; }

        public PeronFoundModel(string username)
        {
            OpenChatCommand = new Command(OpenChat);
            Username = username;
        }

        private void OpenChat()
        {
            SocketCore.Send($"{Username}", Token.INIT_CHAT);
            StaticNavigator.PopAndPush(new MessagePage(Username));
        }
    }
}
