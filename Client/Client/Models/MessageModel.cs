using Newtonsoft.Json;
using System;

namespace Client.Models
{
    public class MessageModel
    {
        public string MessageContent { get; set; }
        public string Username { get; set; }
        public string AvatarImage { get; set; }
        public DateTime Time { get; set; }

        //Used by server
        [JsonConstructor]
        public MessageModel(string user, string message, string time)
        {
            Username = user;
            MessageContent = message;
            time = time.Replace(".", "");
            try
            {
                Time = DateTime.Parse(time);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot parse time {time}" + ex);
            }
        }

        //Used by client
        public MessageModel(string user, string message, DateTime time)
        {
            Username = user;
            MessageContent = message;
            Time = time;
        }
    }
}
