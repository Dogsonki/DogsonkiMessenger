using Newtonsoft.Json;
using System.Text;

namespace Client.Networking.Model;

public class SocketPacket
{
    [JsonProperty("data")]
    public object Data { get; set; }

    [JsonProperty("token")]
    public int Token { get; set; }

    [JsonConstructor]
    public SocketPacket(object data, Token token = Client.Token.EMPTY)
    {
        Data = data;
        Token = (int)token;
    }

    /// <summary>
    /// Returns Data
    /// </summary>
    public object GetEncoded() => Data;

    /// <summary>
    /// Prepares packet to be sended
    /// </summary>
    /// <returns>Prepared packet</returns>
    public byte[] GetPacked() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this) + "$");

    public static bool TryDeserialize(out SocketPacket? packet, string buffer)
    {
        packet = JsonConvert.DeserializeObject<SocketPacket>(buffer);
        return packet is not null;
    }

    public T? ModelCast<T>() => JsonConvert.DeserializeObject<T>(Data.ToString());
}