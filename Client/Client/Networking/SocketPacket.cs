using Newtonsoft.Json;
using System.Text;

namespace Client.Networking.Model
{
    public class SocketPacket
    {
        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("token")]
        public int Token { get; set; }

        private bool IsImage { get; set; } = false;
        private bool IsLastImagePacket { get; set; } = false;

        public object GetEncoded() => Data;

        /// <summary>
        /// Prepares packet to be sended
        /// </summary>
        /// <returns>Prepared packet</returns>
        public byte[] GetPacked()
        {
            if (!IsImage) return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this) + "$");
            else
            {
                if (IsLastImagePacket)
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this) + "$");
                else return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
            }
        }

        [JsonConstructor]
        public SocketPacket(object data, Token token = Networking.Token.EMPTY, bool isImage = false)
        {
            Data = data;
            Token = (int)token;
            IsImage = isImage;
        }

        public SocketPacket(object data,bool isImage, bool isLastPacket,Token token)
        {
            Data = data;
            IsImage = isImage;
            IsLastImagePacket = isLastPacket;
            Token = (int)token;
        }
    }
}
