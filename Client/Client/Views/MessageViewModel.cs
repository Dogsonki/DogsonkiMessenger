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
            for(int i = 0; i < 10; i++)
            {
                AddMessage(new MessageModel("Test", "hii", DateTime.Now));
            }
        }

        /// <summary>
        /// AddMessage only by Client
        /// </summary>
        /// <param name="rev"></param>
        public static void AddMessage(string rev)
        {
            if (rev == null || rev == null)
                return;
            SocketCore.SendRaw(rev);
            Messages.Add(new MessageModel(LocalUser.Username, rev,DateTime.Now));
        }

        /// <summary>
        /// AddMessage only by server
        /// </summary>
        /// <param name="model"></param>
        public static void AddMessage(MessageModel model)
        {
            Messages.Add(model);
        }
    }
}
