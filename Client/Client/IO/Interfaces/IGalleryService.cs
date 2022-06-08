using System.IO;
using System.Threading.Tasks;

namespace Client.IO.Interfaces
{
    public interface IGalleryService
    {
        Task<Stream> GetPickedImage();
    }
}
