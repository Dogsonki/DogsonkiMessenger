using DogsonkiMessenger.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(FileService))]
namespace DogsonkiMessenger.Utility
{
    internal class FileService : IFileService
    {
        public static string GetRootDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        public void CreateFile(string message,string path)
        {
            string _path = Path.Combine(GetRootDir(), path);
            File.WriteAllText(_path, message);
        }
    }
}
