using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace Client.IO
{
    public class StorageIO
    {
        /// <summary>
        /// redo ifInit maybe there is better way to do this 
        /// maybe serialize T if its possible
        /// </summary>
        public static T ReadStorage<T>(string name, object ifInit) where T : IStorage
        {
            bool isInit = false;
            IFileService file = DependencyService.Get<IFileService>();

            if (!file.DirectoryExist(name))
            {
                isInit = true;
                file.CreateDirectory(name);
            }

            if (!file.FileExist($"{name}.json"))
            {
                isInit = true;
                file.CreateFile($"{name}.json");
            }

            if (isInit)
            {
                file.WriteToFile(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ifInit))), name, "Storage");
                return (T)ifInit;
            }
            else
            {
                T i = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(file.ReadFileFromStorage(name, "Storage")));
                return i;
            }
        }

        /// <summary>
        /// Writes and overwrites file if the object is IStorage
        /// </summary>
        public static void Write<T>(T storage, string name) where T : IStorage
        {
            IFileService file = DependencyService.Get<IFileService>();
            file.WriteToFile(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(storage))), name, "Storage");
        }
        /// <summary>
        /// Returns readed buffer of Embeded resource
        /// </summary>
        /// <param name="path">ex. "Client.Pages.B.png" </param>
        /// <param name="typePath">ex. typeof(Client) </param>
        /// <param name="maxBuffer"></param>
        /// <returns></returns>
        public static byte[] ReadEmbededResource(string path,Type typePath,int maxBuffer = 1024*24)
        {
            byte[] buffer;

            var assembly = IntrospectionExtensions.GetTypeInfo(typePath.GetType()).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                buffer = Essential.StreamToBuffer(stream,maxBuffer);
            }
            return buffer;
        }
    }
}
