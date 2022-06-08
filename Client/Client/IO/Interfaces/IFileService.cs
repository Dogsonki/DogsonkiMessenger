using System.IO;

namespace Client.IO
{
    public interface IFileService
    {
        private const string TempLocation = "temp";

        void WriteToFile(MemoryStream stream, string name, string location = TempLocation);
        byte[] ReadFileFromStorage(string name, string location = TempLocation);
        void CreateFile(string name, string location = TempLocation);
        void CreateDirectory(string name, string location = TempLocation);
        bool FileExist(string name, string location = TempLocation);
        bool DirectoryExist(string name, string location = TempLocation);
        string GetPersonalDir();
    }
}
