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

        protected int UpdatedIndex;

        private bool IsImage { get; set; } = false;

        public byte[] GetPacked() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this) + "$");

        [JsonConstructor]
        public SocketPacket(object data, int token=-1)
        {
            //Debug.Write($"Creating Packet: type {content.GetType()} | {(string)content}");\
            Data = data;
            Token = token;
        }
    }
}
