using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models
{
    public class MessageModel
    {
        protected static string[] Cache;

        public string MessageContent { get; set; }
        public string Username { get; set; }
        public string AvatarImage { get; set; }

        public MessageModel(string msg, string user)
        {
            MessageContent = msg;
            Username = user;
        }
    }
}
