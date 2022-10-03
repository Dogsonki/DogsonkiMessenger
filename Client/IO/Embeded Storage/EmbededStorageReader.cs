using Client.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Client.IO;

public static class EmbeddedStorage
{
    public static T Read<T>(Type type, string path)
    {
        try
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(type).Assembly;
            Stream? stream = assembly.GetManifestResourceStream(path);

            if (stream is null)
            {
                throw new Exception($"Embedded file dose not exists {type} | {path}");
            }

            using (var reader = new StreamReader(stream))
            {
                var file = JsonConvert.DeserializeObject(reader.ReadToEnd());
                T? deserialized = ((JObject)file).ToObject<T>();

                if (deserialized is null)
                {
                    throw new Exception("Deserialize Embedded File Null");
                }

                return deserialized;
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