using System.Collections.ObjectModel;

namespace Client.Utility;

internal class Logger
{
    public static ObservableCollection<string> LoggerStack = new ObservableCollection<string>();

    private static List<string> FuncLoggerStack = new List<string>();
    private static List<string> PacketLoggerStack = new List<string>();

    public static void Push(object trace,TraceType type)
    {
        switch (type)
        {
            case TraceType.Packet: PacketLoggerStack.Add(trace.ToString()); break;
            case TraceType.Func: FuncLoggerStack.Add(trace.ToString()); break;
        }
    }
}