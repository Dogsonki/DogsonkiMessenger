using System.IO;

namespace Client.IO
{
    public interface IFileService
    {
        void WriteToFile(MemoryStream stream, string name, string location = "temp");
        byte[] ReadFileFromStorage(string name, string location = "temp");
        void CreateFile(string name, string location = "temp");
        void CreateDirectory(string name, string location = "temp");
        bool FileExist(string name, string location = "temp");
        bool DirectoryExist(string name, string location = "temp");
        string GetPersonalDir();
    }
}
