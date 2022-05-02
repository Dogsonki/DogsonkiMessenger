using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Utility
{
    public interface IFileService
    {
       void CreateFile(string message, string path);
    }
}
