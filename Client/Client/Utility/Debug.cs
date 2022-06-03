using System;
using System.Runtime.CompilerServices;

namespace Client
{
    internal class Debug
    {
        public static void Error(object Content, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null) => Console.WriteLine("[ERROR]: " + path + " at: " + lineNumber + " : " + Content);

        public static void Write(object Content, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null) => Console.WriteLine("[DEBUG]: " + path + " at: " + lineNumber + " : " + Content);

        public static void Clear() => Console.Clear();
    }
}