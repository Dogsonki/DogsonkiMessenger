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
            SocketCore.SendRaw($"{Username}", 3);
            StaticNavigator.PopAndPush(new MessageView(Username));
        }
    }
}
