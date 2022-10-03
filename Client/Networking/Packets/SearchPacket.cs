using Newtonsoft.Json;

namespace Client.Networking.Packets;

public class SearchPacket
{
    [JsonProperty("nick")]
    public string Username { get; set; }
    [JsonProperty("search_groups")]
    public bool SearchGroup { get; set; }

    public SearchPacket(string username, bool searchGroup)
    {
        Username = username;
        SearchGroup = searchGroup;
    }
}