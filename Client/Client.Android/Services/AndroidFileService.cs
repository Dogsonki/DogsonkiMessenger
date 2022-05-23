using System;
using System.IO;
using Client.Utility.Services;

[assembly: Xamarin.Forms.Dependency(typeof(Client.Droid.AndroidFileService))]
namespace Client.Droid
{
    public class AndroidFileService : IFileService
    {
        public static string GetPersonalDir(string location)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            documentsPath = Path.Combine(documentsPath, "Orders", location);
            return documentsPath;
        }

        public byte[] ReadFileFromStorage(string name, string location = "temp")
        {
            var documentsPath = GetPersonalDir(location);
            string filePath = Path.Combine(documentsPath, name);

            byte[] buffer = File.ReadAllBytes(filePath);

            return buffer;
        }

        public void SaveFileToStorage(MemoryStream stream, string name, string location = "temp")
        {
            var documentsPath = GetPersonalDir(location);

            if(!Directory.Exists(documentsPath))
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
    }
}
