using System;
using System.Runtime.CompilerServices;

namespace Client.Utility
{
    internal class Debug
    {
        public static void Error(object Content, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR]: " + path + " at: " + lineNumber + " : " + Content);
            Console.ResetColor();
        }

        public static void Write(object Content, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[DEBUG]: " + path + " at: " + lineNumber + " : " + Content);
            Console.ResetColor();
        }

        public static void Clear() => Console.Clear();

    }
}