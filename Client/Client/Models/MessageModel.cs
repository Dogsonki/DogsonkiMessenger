using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Client.Models
{
    public class MessageModel
    {
        public List<MessageModel> m_Cache { get; set; }
        protected string[] Cached;

        #region STYLE

        public int m_Row { get; set; } = 0;

        #endregion

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
            time = time.Replace(".", "");
            //Python sends %%%%%%.%%%%% for same reason ...

            try
            {
                Time = DateTime.Parse(time);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Cannot parse time {time}" +ex);
            }
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
