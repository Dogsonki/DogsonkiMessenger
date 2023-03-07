using Client.Networking.Core;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Client.Networking.Models;

public class SocketPacket
{
    [JsonProperty("data")]
    public object Data { get; }

    [JsonProperty("token")]
    public int PacketToken { get; }

    [JsonIgnore]
    private const char EndOfPacket = '$';

    [JsonIgnore]
    public string? PacketError { get; set; }

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

    public string Serialize() => JsonConvert.SerializeObject(this);

    public static bool TryDeserialize(out SocketPacket packet, string buffer)
    {
        packet = JsonConvert.DeserializeObject<SocketPacket>(buffer);
        return packet is not null;
    }

    private static readonly JsonSerializer jsonSerializer = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
    };

    public T? Deserialize<T>()
    {
        if(Data.GetType() == typeof(JObject))
        {
            return ((JObject)Data).ToObject<T>(jsonSerializer);
        }

        T? model = JsonConvert.DeserializeObject<T>(Convert.ToString(Data),
            new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

        if(model is null)
        {
            PacketError = "Deserialize error packet";
        }

        return model;
    }
    
    public int ToInt()
    {
        Type type = Data.GetType();

        if(type == typeof(int))
            return (int)Data;

        if(type == typeof(long) || type == typeof(short))
        {
            return Convert.ToInt32(Data);
        }

        return int.Parse(Convert.ToString(Data));
    }

    public override string? ToString()
    {
        return Data.ToString();
    }

    public T? ModelCast<T>() => JsonConvert.DeserializeObject<T>(Data.ToString());
}