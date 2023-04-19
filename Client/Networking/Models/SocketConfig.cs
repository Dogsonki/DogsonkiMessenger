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

    public SocketConfig() 
    {
        Ip = string.Empty;
        Port = 0;
    }

    public void ReadConfig()
    {
        SocketConfig? config = EmbeddedStorage.Read<SocketConfig>(typeof(SocketConfig), "Client.Networking.SocketConfig.json");

        if (config is null)
        {
            throw new Exception("Client.Networking.SocketConfig.json dose not exist");
        }

        Ip = config.Ip;
        Port = config.Port;
    }
}