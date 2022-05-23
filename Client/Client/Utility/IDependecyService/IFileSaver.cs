using System.IO;

namespace Client.Utility.Services
{
    public interface IFileService
    {
        void SaveFileToStorage(MemoryStream stream, string name, string location = "temp");
        byte[] ReadFileFromStorage(string name, string location = "temp");
    }
}
