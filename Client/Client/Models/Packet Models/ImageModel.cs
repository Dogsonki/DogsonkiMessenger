using Newtonsoft.Json;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Client.Models
{
    public class ImageModel
    {
        public static Dictionary<uint,ImageModel> Images = new Dictionary<uint,ImageModel>();

        public uint Id { get; set; }    
        public byte[] ImageData { get; set; }

        public Image Image { get; set; }    

        [JsonConstructor]
        public ImageModel(byte[] imageData, uint id)
        {
            ImageData= imageData;
            Id = id;
        }

        public void Attach()
        {
            Images.TryGetValue(Id, out ImageModel image);
        }
    }
}
