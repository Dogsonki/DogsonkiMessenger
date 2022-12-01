#undef USE_VS_DEBUGGER

using System.Runtime.CompilerServices;

namespace Client.Utility;

public class Debug
{
    public static void Error(object Content, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
    {
#if DEBUG && !USE_VS_DEBUGGER
        Console.WriteLine("[ERROR]: " + path + " at: " + lineNumber + " : " + Content);
#elif USE_VS_DEBUGGER
        System.Diagnostics.Trace.WriteLine("[ERROR]: " + path + " at: " + lineNumber + " : " + Content);
#endif

    }

    public static void Write(object Content, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
    {
#if DEBUG && !USE_VS_DEBUGGER
        Console.WriteLine("[DEBUG]: " + path + " at: " + lineNumber + " : " + Content);
#elif USE_VS_DEBUGGER
        System.Diagnostics.Trace.WriteLine("[DEBUG]: " + path + " at: " + lineNumber + " : " + Content);
#endif
    }
}