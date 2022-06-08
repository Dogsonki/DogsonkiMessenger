using Client.Networking;
using Xamarin.Forms;

namespace Client.Models
{
    public class PeronFoundModel
    {
        public UserModel User { get; set; }
        public Command OpenChatCommand { get; }
        public ImageSource Image { get; set; }

        public PeronFoundModel(UserModel user)
        {
            OpenChatCommand = new Command(OpenChat);
            User = user;
        }

        private void OpenChat()
        {
            SocketCore.Send($"{User.Name}", Token.INIT_CHAT);
            StaticNavigator.PopAndPush(new MessagePage(User.Name));
        }
    }
}
