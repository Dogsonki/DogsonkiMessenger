using Client.Models;
using Client.Networking;
using System;
using System.Collections.ObjectModel;


namespace Client.Views
{
    public class MessagePageView
    {
        public static ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();

        public MessagePageView()
        {
        }

        /// <summary>
        /// AddMessage only by Client
        /// </summary>
        /// <param name="rev"></param>
        public static void Test_AddMessage(string rev)
        {
            Messages.Add(new MessageModel(LocalUser.Username, rev, DateTime.Now));
        }

        /// <summary>
        /// AddMessage only by Client
        /// </summary>
        /// <param name="rev"></param>
        public static void AddMessage(string rev)
        {
            if (rev == null || rev == null)
                return;
            SocketCore.Send(rev);
            var r = new MessageModel(LocalUser.Username, rev, DateTime.Now);
            Messages.Add(r);
            MessagePage.ScrollToBottom(r);
        }

        /// <summary>
        /// AddMessage only by server
        /// </summary>
        /// <param name="model"></param>
        public static void AddMessage(MessageModel model)
        {
            Messages.Add(model);
            MessagePage.ScrollToBottom(model);
        }
    }
}
