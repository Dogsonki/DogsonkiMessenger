using Client.Networking.Core;
using Newtonsoft.Json;
using System.Text;

namespace Client.Networking.Models;

public class SocketPacket
{
    [JsonProperty("data")]
    public object Data { get; set; }

    [JsonProperty("token")]
    public int PacketToken { get; set; }

    [JsonIgnore]
    private const char EndOfPacket = '$';

    [JsonConstructor]
    public SocketPacket(object data, Token token = Token.EMPTY)
    {
        Data = data;
        PacketToken = (int)token;
    }

    /// <summary>
    /// Returns Encoded Data
    /// </summary>
    public object GetEncoded() => Data;

    /// <summary>
    /// Prepares packet to be sended
    /// </summary>
    /// <returns>Prepared packet</returns>
    public byte[] GetPacked() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this) + EndOfPacket);

    public static bool TryDeserialize(out SocketPacket? packet, string buffer)
    {
        packet = JsonConvert.DeserializeObject<SocketPacket>(buffer);
        return packet is not null;
    }

    public T? ModelCast<T>() => JsonConvert.DeserializeObject<T>(Data.ToString());
}