using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models.Packet_Models
{
    internal class ImageModel
    {
        [JsonProperty("image")]
        public byte[] Image;
    }
}
