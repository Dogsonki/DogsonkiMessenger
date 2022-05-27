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

        /// <summary>
        /// Prepares packet to be sended
        /// </summary>
        /// <returns>Prepared packet</returns>
        public byte[] GetPacked() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this) + "$");

        [JsonConstructor]
        public SocketPacket(object data, int token = -1, bool isImage = false)
        {
            Data = data;
            Token = token;
            IsImage = isImage;
        }
    }
}
