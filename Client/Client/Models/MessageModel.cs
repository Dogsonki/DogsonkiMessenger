using Newtonsoft.Json;
using System;

namespace Client.Models
{
    public class MessageModel
    {
        protected static string[] Cache;

        public string MessageContent { get; set; }
        public string Username { get; set; }
        public string AvatarImage { get; set; }
        public DateTime Time { get; set; }

        [JsonConstructor]
        public MessageModel(string user, string message,string time)
        {
            Username = user;    
            MessageContent = message;
            Time = DateTime.Parse(time);
            Console.WriteLine(Time);
        }   

        public MessageModel(string user, string message, DateTime time)
        {
            Username = user;
            MessageContent = message;
            Time = time;
        }
    }
}
