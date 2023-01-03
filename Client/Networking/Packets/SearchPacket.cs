using Newtonsoft.Json;

namespace Client.Networking.Packets;

public class SearchPacket
{
    [JsonProperty("nick")]
    public string Name { get; set; }
    [JsonProperty("search_groups")]
    public bool SearchGroup { get; set; }

    public SearchPacket(string name, bool searchGroup)
    {
        Name = name;
        SearchGroup = searchGroup;
    }
}