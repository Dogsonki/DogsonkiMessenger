using System.Collections.ObjectModel;

namespace Client.Utility;

public class Logger
{
    public static ObservableCollection<LogTemplate> LoggerStack { get; set; } = new ObservableCollection<LogTemplate>();

    private static List<string> FuncLoggerStack = new List<string>();
    private static List<string> PacketLoggerStack = new List<string>();

    public static void Push(object trace,TraceType type,LogLevel level)
    {
        Debug.Write(trace);

        switch (type)
        {
            case TraceType.Packet: PacketLoggerStack.Add(trace.ToString()); break;
            case TraceType.Func: FuncLoggerStack.Add(trace.ToString()); break;
        }
        LoggerStack.Add(new LogTemplate(trace.ToString(), level));
    }
}

public enum LogLevel
{
    Error,
    Warning,
    Debug
}

public struct LogTemplate
{
    public string message { get; set; }
    public Color color { get; set; }

    private static Color ErrorColor = Color.FromRgb(255, 50, 50);
    private static Color WarningColor = Color.FromRgb(255, 205, 0);
    private static Color DebugColor = Color.FromRgb(255, 255, 255);

    public LogTemplate(string msg, LogLevel level)
    {
        color = GetColor(level);
        message = msg;
    }

    private static Color GetColor(LogLevel level) 
    {
        switch (level)
        {
            case LogLevel.Error: return ErrorColor; 
            case LogLevel.Warning: return WarningColor;
            case LogLevel.Debug: return DebugColor;
        }
        return Color.FromRgb(0, 0, 0);
    }
}