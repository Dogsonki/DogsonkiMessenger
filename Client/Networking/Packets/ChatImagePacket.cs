using Newtonsoft.Json;

namespace Client.Networking.Packets;

public class ChatImagePacket
{
    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("file_format")]
    public string FileFormat { get; set; }

    public ChatImagePacket(string path,string file_format)
    {
        Path = path;
        FileFormat = file_format;
    }
}