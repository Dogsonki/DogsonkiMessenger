using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Client.IO
{
    public class EmbededStorage
    {
        public static T Read<T>(Type type,string Path)
        {
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(type.GetType()).Assembly;
                Stream stream = assembly.GetManifestResourceStream(Path);
                using (var reader = new StreamReader(stream))
                {
                    var DesT = JsonConvert.DeserializeObject(reader.ReadToEnd());
                    return (T)((JObject)DesT).ToObject(typeof(T));
                }
            }
            catch(Exception ex)
            {
                if(ex.GetType() == typeof(NullReferenceException))
                {
                    Debug.Error(ex);
                }
                return default;
            }
        } 
    }
}
