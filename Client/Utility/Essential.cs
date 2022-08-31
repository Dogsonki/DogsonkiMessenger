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
                Logger.Push(ex,TraceType.Func,LogLevel.Error);
            }
            return dest;
        }

        public static DateTime UnixToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}