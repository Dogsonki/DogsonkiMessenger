using Newtonsoft.Json;
using System.Text;

namespace Client.Networking.Model;


public class SocketPacket
{
    [JsonProperty("data")]
    public object Data { get; set; }

    [JsonProperty("token")]
    public int Token { get; set; }

    //Images are sent as normal packets

    public object GetEncoded() => Data;

    /// <summary>
    /// Prepares packet to be sended
    /// </summary>
    /// <returns>Prepared packet</returns>
    public byte[] GetPacked()
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this) + "$");
    }

    [JsonConstructor]
    public SocketPacket(object data, Token token = Client.Token.EMPTY, bool isImage = false)
    {
        Data = data;
        Token = (int)token;
    }

    public SocketPacket(object data, bool isImage, bool isLastPacket, Token token)
    {
        Data = data;
        Token = (int)token;
    }

    public static bool TryDeserialize(out SocketPacket packet, string buffer)
    {
        try
        {
            packet = JsonConvert.DeserializeObject<SocketPacket>(buffer);
        }
        catch (Exception ex)
        {
            packet = null;
            Debug.Error(ex);

            return false;
        }
        return true;
    }
}