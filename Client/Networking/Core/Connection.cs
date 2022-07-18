using Client.Networking.Model;
using Client.Utility;
using System.Net.Sockets;

namespace Client.Networking.Core;

public class Connection
{
    public static TcpClient Client { get; set; }
    public static NetworkStream Stream { get; set; }
    public static SocketConfig Config { get; set; }

    protected static Thread ReciveThread { get; private set; }
    protected static Thread ManageSendingQueue { get; private set; }

    public static bool IsConnected { get; private set; }
    public static bool IsInitialized { get; private set; } = false;
    public static int MaxBuffer = 1024 * 128;
    public static bool IsConnecting { get; protected set; }

    private static List<Action> OnConnectionActions = new List<Action>();

    public static void AddOnConnection(Action action) => OnConnectionActions.Add(action);

    public static void Connect()
    {
        if (IsConnecting)
            return;
        IsConnecting = true;
        try
        {
            Config = SocketConfig.ReadConfig();
            Client = new TcpClient(Config.Ip,Config.Port);
            Stream = Client.GetStream();
        }
        catch(Exception)
        {
            IsConnecting = false;
            return;
        }

        ReciveThread.Start();
        ManageSendingQueue.Start();

        IsInitialized = true;
        IsConnected = true;
        IsConnecting = false;

        foreach(Action act in OnConnectionActions)
        {
            act?.Invoke();
        }
    }

    public Connection(ThreadStart ReciveHandler, ThreadStart SendingHandler) 
    {
        ReciveThread = new Thread(ReciveHandler);
        ManageSendingQueue = new Thread(SendingHandler);

        try
        {
            Connect();
        }
        catch(Exception ex)
        {
            Type ERROR_TYPE = ex.GetType();
            if (ERROR_TYPE == typeof(SocketException))
            {
                Debug.Error("UNABLE_TO_CONNECT");//its ok
            }
            else if (ERROR_TYPE == typeof(ArgumentNullException) || ERROR_TYPE == typeof(ArgumentOutOfRangeException))
            {
                Debug.Error("SOCKETCONFIG_DESERIALIZE_ERROR " + ex);
            }
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }

    public static bool AbleToSend()
    {
        if (Client == null || Stream == null)
        {
            Logger.Push("Client or stream is null", TraceType.Packet, LogLevel.Error);
            return false;
        }

        return true;
    }

}
