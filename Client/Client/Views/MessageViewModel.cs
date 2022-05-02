using DogsonkiMessenger.Models;
using Client.Networking;
using System.Collections.ObjectModel;

namespace Client.Views
{
    public class MessageViewModel
    {
        public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();
        public static ObservableCollection<MessageModel> InstanceMessages;

        public MessageViewModel()
        {
            InstanceMessages = Messages;
        }

        public static void AddMessage(string message,string username)
        {
            if (message == null || username == null)
                return;
                //Asster
            InstanceMessages.Add(new MessageModel(message, username));
        }

        private static void SendMessage(string message,string username)
        {
            SocketCore.SendRaw("SendingMessage");
            SocketCore.SendRaw(username);
            SocketCore.SendR(null, message, 0004, 0004);
        }
    }
}
