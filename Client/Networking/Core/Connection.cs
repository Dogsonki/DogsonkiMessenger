using Client.Networking.Model;
using Client.Utility;
using System.Net.Sockets;

namespace Client.Networking.Core;

public class Connection
{
    public static TcpClient Client { get; set; }
    public static NetworkStream Stream { get; set; }
    public static SocketConfig Config { get; set; }

    public static bool IsConnected { get; private set; }
    public static bool IsInitialized { get; private set; } = false;

    public const int MAX_BUFFER_SIZE = 1024 * 10;

    public static bool IsConnecting { get; protected set; }

    private static List<Action> OnConnectionActions = new List<Action>();

    public static void AddOnConnection(Action action) => OnConnectionActions.Add(action);

    public static async Task Connect()
    {
        try
        {
            Config = SocketConfig.ReadConfig();

            if (Config is null) throw new Exception("SOCKET_CONFIG_READ_EXCEPTION");

            Client = new TcpClient();

            await Client.ConnectAsync(Config.Ip, Config.Port).ContinueWith((_) =>
            {
                Stream = Client.GetStream();

                foreach (Action act in OnConnectionActions)
                {
                    act?.Invoke();
                }
            });
        }
        catch (Exception ex) //Logging this exception can expose ip and port to server :/
        {
            Debug.Write(ex); 
        }
    }

    public static bool AbleToSend()
    {
        if (Client == null || Stream == null || !Client.Connected)
        {
            Logger.Push($"Stream: {Stream is null} Client: {Client is null} Connected? {Client?.Connected}", TraceType.Packet, LogLevel.Error);
            return false;
        }
        return true;
    }
}
