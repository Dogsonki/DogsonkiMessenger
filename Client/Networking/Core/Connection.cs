using Client.Networking.Model;
using System.Net.Sockets;

namespace Client.Networking.Core;

public class Connection
{
    public static TcpClient Client { get; set; }
    public static NetworkStream Stream { get; set; }
    public static SocketConfig Config { get; set; }

    protected static Thread ReciveThread { get; set; }
    protected static Thread ManageSendingQueue { get; set; }

    public static bool IsConnected { get; private set; }
    public static bool IsInitialized { get; private set; } = false;
    public static int MaxBuffer = 1024 * 128;

    private static void Connect()
    {
        try
        {
            Client = new TcpClient(Config.Ip, Config.Port);
        }
        catch(Exception)
        {
            throw; //TODO: Check if it's thread safe
        }

        ReciveThread.Start();
        ManageSendingQueue.Start();

        IsInitialized = true;
        IsConnected = true;
    }

    public Connection(ThreadStart ReciveHandler, ThreadStart SendingHandler) 
    {
        ReciveThread = new Thread(ReciveHandler);
        ManageSendingQueue = new Thread(SendingHandler);

        try
        {
            Config = SocketConfig.ReadConfig();
            Task.Run(Connect);
        }
        catch(Exception ex)
        {
            Type ERROR_TYPE = ex.GetType();
            if (ERROR_TYPE == typeof(SocketException))
            {
                Debug.Write("UNABLE_TO_CONNECT");//its ok
            }
            else if (ERROR_TYPE == typeof(ArgumentNullException) || ERROR_TYPE == typeof(ArgumentOutOfRangeException))
            {
                Debug.Error("SOCKETCONFIG_DESERIALIZE_ERROR " + ex);
            }
            Debug.Write(ex);
        }
    }

    public static bool AbleToSend()
    {
        if (Client == null || Stream == null)
        {
            Debug.Error("Client is null");
            return false;
        }

        if (!IsConnected)
        {
            Debug.Error("Client is not connected to server");
            return false;
        }

        if (!Client.Connected)
        {
            Debug.Error("Client disconnected form server");
            return false;
        }
        return true;
    }

}
