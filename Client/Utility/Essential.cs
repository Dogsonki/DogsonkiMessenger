using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client.Utility
{
    public static class Essential
    {
        public static byte[] StreamToBuffer(this Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        [Obsolete]
        public static T? ModelCast<T>(this object data)
        {
            T dest = default;
            Type dataType = data.GetType();
            try
            {
                if (dataType == typeof(JObject))
                {
                    dest = ((JObject)data).ToObject<T>();
                }
                else if (dataType == typeof(JArray))
                {
                    dest = ((JArray)data).ToObject<T>();
                }
                else if(dataType == typeof(string))
                {
                    data = JsonConvert.DeserializeObject<T>((string)data);
                }
            }
            catch (Exception ex)
            {
                Logger.Push(ex, LogLevel.Error);
            }
            return dest;
        }

        public static DateTime UnixToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static byte[] GetImageBuffer(string imageBuffer)
        {
            return Convert.FromBase64String(imageBuffer); 
        }

        public static string DateTimeToFactored(DateTime time)
        {
            DateTime currentTime = DateTime.Now;

            if (currentTime.Year == time.Year && currentTime.Month == time.Month && currentTime.Day == time.Day)
            {
                return $"Today {time.ToString("h:mm tt")}";
            }
            else if(wasYesterday(time))
            {
                return $"Yesterday at {time.ToString("h:mm tt")}";
            }
            else if(currentTime.Year == time.Year && currentTime.Month == time.Month && currentTime.Day != time.Day && currentTime.Day != time.Day-1)
            {
                return $"{DateTime.Now.AddDays(-time.Day).Day} days ago at {time.ToString("h:mm tt")}";
            }
            else
            {
                return $"{time.Day}/{time.Month}/{time.Year} at {time.ToString("h:mm tt")}";
            }

            bool wasYesterday(DateTime time)
            {
                return DateTime.Now.ToString("MM/dd/yyy") == time.ToString("MM/dd/yyy");
            }
        }

        public static string DateTimeToFactored(double time) => DateTimeToFactored(UnixToDateTime(time));
    }
}