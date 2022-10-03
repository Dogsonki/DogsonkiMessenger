using Newtonsoft.Json;

namespace Client.Networking.Packets;

[Serializable]
public class ChatImagePacket
{
    [JsonProperty("path")]
    public string Path { get; set; }
    [JsonProperty("file_format")]
    public string FileFormat { get; set; }

    [JsonConstructor]
    public ChatImagePacket(string path,string file_format)
    {
        Path = path;
        FileFormat = file_format;
    }
}