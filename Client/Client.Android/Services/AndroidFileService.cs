using Client.IO;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(Client.Droid.AndroidFileService))]
namespace Client.Droid
{
    public class AndroidFileService : IFileService
    {
        public static string GetPersonalDir(string location)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, "Storage", location);
            return path;
        }
        public bool FileExist(string name, string location = "temp") => File.Exists(Path.Combine(GetPersonalDir(location), name));
        public bool DirectoryExist(string name, string location = "temp") => Directory.Exists(Path.Combine(GetPersonalDir(location), name));
        public void CreateFile(string name, string location) => File.Create(Path.Combine(GetPersonalDir(location), name));
        public byte[] ReadFileFromStorage(string name, string location = "temp")
        {
            Device.InvokeOnMainThreadAsync(async () => await RequestPermissionAsync());

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

        public void WriteToFile(MemoryStream stream, string name, string location = "temp")
        {
            var documentsPath = GetPersonalDir(location);

            if (!Directory.Exists(documentsPath))
                Directory.CreateDirectory(documentsPath);

            string filePath = Path.Combine(documentsPath, name);

            byte[] bArray = new byte[stream.Length];
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (stream)
                {
                    stream.Read(bArray, 0, (int)stream.Length);
                }
                int length = bArray.Length;
                fs.Write(bArray, 0, length);
            }
        }
        public void CreateDirectory(string name, string location = "temp") => Directory.CreateDirectory(Path.Combine(GetPersonalDir(location), name));
        public string GetPersonalDir() => GetPersonalDir();
    }
}
