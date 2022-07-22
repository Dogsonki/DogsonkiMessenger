using Client.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Client.IO
{
    public static class EmbededStorage
    {
        public static T Read<T>(Type type, string Path)
        {
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(type).Assembly;
                Stream stream = assembly.GetManifestResourceStream(Path);
                using (var reader = new StreamReader(stream))
                {
                    var DesT = JsonConvert.DeserializeObject(reader.ReadToEnd());
                    return (T)((JObject)DesT).ToObject(typeof(T));
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(NullReferenceException))
                {
                    Logger.Push(ex, TraceType.Func, LogLevel.Error);
                }
                return default;
            }
        }
    }
}
