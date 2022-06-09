using System;
using System.Runtime.CompilerServices;

namespace Client
{
    internal class Debug
    {
        public static void Error(object Content, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
        {
#if DEBUG
            Console.WriteLine("[ERROR]: " + path.Substring(60) + " at: " + lineNumber + " : " + Content);
#endif
        }

        public static void Write(object Content, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
        {
#if DEBUG
            Console.WriteLine("[DEBUG]: " + path.Substring(60) + " at: " + lineNumber + " : " + Content);
#endif
        }
    }
}