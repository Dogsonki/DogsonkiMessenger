using Client.Models;
using Newtonsoft.Json.Linq;

namespace Client.Utility
{
    public class Essential
    {
        public static byte[] StreamToBuffer(Stream stream, long maxBuffer = 12 * 1024)
        {
            byte[] buffer = new byte[maxBuffer];
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

        public static T ModelCast<T>(object data)
        {
            T dest = default;
            try
            {
                if (data.GetType() == typeof(JObject))
                {
                    dest = ((JObject)data).ToObject<T>();
                }
                else if (data.GetType() == typeof(JArray))
                {
                    dest = ((JArray)data).ToObject<T>();
                }
            }
            catch(Exception ex)
            {
                Debug.Error(ex);
            } 
            return dest;
        }
    }
}