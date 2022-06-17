using Client.IO;
using System.Text;

namespace Client;

 public class AndroidFileService : IFileService
{
    private const string TempLocation = "temp";

    public static string GetPersonalDir(string location) => FileSystem.CacheDirectory + $"/{location}";

    /// <summary>
    /// Returns false if directory was not present in location
    /// </summary>
    public bool CreateDirectoryIfNotExist(string name,string location= TempLocation)
    {
        if (!DirectoryExist(name, location))
        {
            CreateDirectory(name, location);
            return true;
        }
        return false;
    }
    public void DeleteFile(string location) => File.Delete(GetPersonalDir(location));
    /// <summary>
    ///  Returns true if file was present in location
    /// </summary>
    public bool CreateFileIfNotExist(string location,string content = null)
    {
        if (!File.Exists(GetPersonalDir(location)))
        {
            if (!string.IsNullOrEmpty(content))
            {
                using (Stream stream = File.Open(GetPersonalDir(location), FileMode.OpenOrCreate))
                {
                    byte[] Buffer = Encoding.UTF8.GetBytes(content);
                    stream.Write(Buffer, 0, Buffer.Length);
                }
            }
            else
            {
                using (Stream stream = File.Open(GetPersonalDir(location), FileMode.Create)) {}//Thread safe creating file
            }
            return false;
        }
        return true;
    }
    public bool FileExist(string location) => File.Exists(GetPersonalDir(location));
    public bool DirectoryExist(string name, string location = TempLocation) => Directory.Exists(Path.Combine(GetPersonalDir(location), name));
    public void CreateFile(string location) => File.Create(GetPersonalDir(location));
    public byte[] ReadFileFromStorage(string name, string location = TempLocation)
    {
        MainThread.InvokeOnMainThreadAsync(() => RequestPermissionAsync());

        string filePath = Path.Combine(GetPersonalDir(location), name);

        byte[] buffer = File.ReadAllBytes(filePath);

        return buffer;
    }

    //API < 21 will ask for permissions
    private static async Task RequestPermissionAsync()//TODO: make it async and ask before appEntry
    {
        var write = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (write != PermissionStatus.Granted)
        {
            write = await Permissions.RequestAsync<Permissions.StorageWrite>();
        }

        var read = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (read != PermissionStatus.Granted)
        {
            read = await Permissions.RequestAsync<Permissions.StorageRead>();
        }
    }

    public void WriteToFile(MemoryStream stream, string location)
    {
        byte[] bArray = new byte[stream.Length];
        using (FileStream fs = new FileStream(location, FileMode.OpenOrCreate))
        {
            using (stream)
            {
                stream.Read(bArray, 0, (int)stream.Length);
            }
            int length = bArray.Length;
            fs.Write(bArray, 0, length);
        }
    }
    public void WriteToFile(string content, string location)
    {
        File.WriteAllBytes(GetPersonalDir(location),Encoding.UTF8.GetBytes(content));
    }
    public void WriteToFile(byte[] content, string location)
    {
        File.WriteAllBytes(GetPersonalDir(location), content);
    }
    public void CreateDirectory(string name, string location = TempLocation) => Directory.CreateDirectory(Path.Combine(GetPersonalDir(location), name));
    public string GetPersonalDir() => GetPersonalDir();
}
