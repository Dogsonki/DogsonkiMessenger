using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Client.Networking.Config
{
    public class SocketConfig
    {
        [JsonProperty("Socket_IP")]
        public string Ip;
        [JsonProperty("Socket_PORT")]
        public int Port;

        public static SocketConfig ReadConfig()
        {
            string config = string.Empty;
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(SocketConfig)).Assembly;
                Stream stream = assembly.GetManifestResourceStream("Client.Networking.SocketConfig.json");

                using (var reader = new StreamReader(stream))
                {
                    config = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (!string.IsNullOrEmpty(config))
            {
                return JsonConvert.DeserializeObject<SocketConfig>(config);
            }
            return null;
        }
    }
}
