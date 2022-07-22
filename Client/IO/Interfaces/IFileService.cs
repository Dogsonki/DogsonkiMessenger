namespace Client.IO;

public interface IFileService
{
    private const string TempLocation = "temp";

    void WriteToFile(MemoryStream stream, string location = TempLocation);
    void WriteToFile(byte[] content, string location);
    void WriteToFile(string content, string location);
    byte[] ReadFileFromStorage(string name, string location = TempLocation);
    void DeleteFile(string location);
    void CreateFile(string location);
    void CreateDirectory(string name, string location = TempLocation);
    bool FileExist(string location);
    bool DirectoryExist(string name, string location = TempLocation);
    string GetPersonalDir();
}