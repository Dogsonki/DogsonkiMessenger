using Newtonsoft.Json;
using Client.IO;

namespace Client.Networking.Models;

[Serializable]
public class SocketConfig
{
    [JsonProperty("Socket_IP")]
    public string Ip;
    [JsonProperty("Socket_PORT")]
    public int Port;

    [JsonConstructor]
    public SocketConfig(string ip, int port)
    {
        Ip = ip;
        Port = port;
    }

    public static SocketConfig ReadConfig()
    {
        return EmbeddedStorage.Read<SocketConfig>(typeof(SocketConfig), "Client.Networking.SocketConfig.json");
    }
}