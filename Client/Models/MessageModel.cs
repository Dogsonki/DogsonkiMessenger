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
        public MessageModel(string user, string message, double time)
        {
            Username = user;
            MessageContent = message;
            try
            {
                Time = UnixToDateTime(time);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot parse time {time}" + ex);
            }
        }

        public static DateTime UnixToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
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
