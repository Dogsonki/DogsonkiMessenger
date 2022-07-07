using Client.IO;
using Newtonsoft.Json;

namespace Client.Networking.Model;

public class SocketConfig
{
    [JsonProperty("Socket_IP")]
    public string Ip;
    [JsonProperty("Socket_PORT")]
    public int Port;

    public SocketConfig(string ip, int port)
    {
        Ip = ip;
        Port = port;
    }

    public static SocketConfig ReadConfig()
    {
        return EmbededStorage.Read<SocketConfig>(typeof(SocketConfig), "Client.Networking.SocketConfig.json");
    }
}