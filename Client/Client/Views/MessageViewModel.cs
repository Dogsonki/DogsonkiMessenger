using Client.Models;
using Client.Networking;
using System;
using System.Collections.ObjectModel;

namespace Client.Views
{
    public class MessageViewModel
    {
        public static ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();

        public MessageViewModel()
        {
        }

        public static void AddMessage(string rev)
        {
            if (rev == null || rev == null)
                return;
            SocketCore.SendRaw(rev);
                //Assert
            Messages.Add(new MessageModel(LocalUser.Username, rev,DateTime.Now));
        }

        public static void AddMessage(MessageModel model)
        {
            Messages.Add(model);
        }
    }
}
