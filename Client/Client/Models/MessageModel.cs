using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Client.Models
{
    public class MessageModel
    {
        public List<MessageModel> m_Cache { get; set; }
        protected string[] Cached;

        public string MessageContent { get; set; }
        public string Username { get; set; }
        public string AvatarImage { get; set; }
        public DateTime Time { get; set; }

        //Used by server
        [JsonConstructor]
        public MessageModel(string user, string message,string time)
        {
            Username = user;    
            MessageContent = message;
            Time = DateTime.Parse(time);
        }   

        //Used by local
        public MessageModel(string user, string message, DateTime time)
        {
            Username = user;
            MessageContent = message;
            Time = time;
        }

        public void AddToCache() 
        {
            m_Cache.Add(this);
        }
    }
}
