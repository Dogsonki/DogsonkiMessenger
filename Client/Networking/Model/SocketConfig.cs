using Client.IO;
using Client.Networking.Core;
using Newtonsoft.Json;

namespace Client.Networking.Model;

public class SocketConfig
{
    [JsonProperty("Socket_IP")]
    public string Ip;
    [JsonProperty("Socket_PORT")]
    public int Port;

    public static SocketConfig ReadConfig()
    {
        SocketConfig config = null;
        config = EmbededStorage.Read<SocketConfig>(typeof(SocketCore), "Client.Networking.SocketConfig.json");

        try
        {
        }
        catch (Exception ex)
        {
            Debug.Write(ex);
            return null;
        }

        return config;
    }

    public SocketConfig(string ip, int port)
    {
        Ip = ip;
        Port = port;
    }
}