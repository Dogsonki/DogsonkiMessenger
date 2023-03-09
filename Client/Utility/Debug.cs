#undef USE_VS_DEBUGGER

using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Client.Utility;

public class Debug
{
    public static void Error(object Content, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
    {
#if DEBUG && !USE_VS_DEBUGGER
        Console.WriteLine("[ERROR]: " + path + " at: " + lineNumber + " : " + GetSerialized(Content));
#elif USE_VS_DEBUGGER
        System.Diagnostics.Trace.WriteLine("[ERROR]: " + path + " at: " + lineNumber + " : " + Content);
#endif

    }

    public static void Assert(bool condition, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
    {
#if DEBUG
        if (condition)
        {
            throw new Exception($"Assert validated {path} at {lineNumber}");
        }
#endif
    }

    public static void ThrowIfNull(object? obj, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
    {
#if DEBUG
        if (obj == null)
        {
            throw new Exception($"Assert validated {path} at {lineNumber}");
        }
#endif
    }

    public static void Write(object? Content, bool printPath = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = "")
    {
#if DEBUG && !USE_VS_DEBUGGER
        if(printPath){
            Console.WriteLine($"[DEBUG]: {path} at {lineNumber} {GetSerialized(Content)}");
        }
        else {
            Console.WriteLine($"[DEBUG]: {GetSerialized(Content)}");
        }
#elif USE_VS_DEBUGGER
        System.Diagnostics.Trace.WriteLine("[DEBUG]: " + path + " at: " + lineNumber + " : " + Content);
#endif
    }

    private static string? GetSerialized(object? content)
    {
        if(content is null) {
            return string.Empty;
        }
        //Check if content is ready to serialize
        try
        {
            Type type = content.GetType();
            object? attribute = type.GetCustomAttributes(typeof(SerializableAttribute), true).FirstOrDefault();

            if (attribute is not null)
            {
                return JsonConvert.SerializeObject(content);
            }
        }
        catch(Exception ex) 
        {
            return ex.ToString();
        }

        return (string)content;
    }
}