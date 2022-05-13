using Xamarin.Forms;
using Client.Networking;
using Client.Utility;

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
            SocketCore.SendR((string _) => StaticNavigator.PopAndPush(new MessageView()), $"0-{Username}", "0003");
        }
    }
}
