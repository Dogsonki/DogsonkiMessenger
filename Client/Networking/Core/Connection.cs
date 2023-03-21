using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Client.Utility;
using Client.Networking.Models;

namespace Client.Networking.Core;

public class Connection
{
    protected static TcpClient Client { get; set; }
    protected static SslStream Stream { get; set; }
    protected static SocketConfig Config { get; set; }

    protected static bool IsConnected { get; private set; }
    protected static bool IsInitialized { get; private set; } = false;

    protected const int MAX_BUFFER_SIZE = 1024 * 48;

    protected static bool IsConnecting { get; set; }

    private static List<Action> OnConnectionActions = new List<Action>();

    public static void AddOnConnection(Action action) => OnConnectionActions.Add(action);

    public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if(sslPolicyErrors != SslPolicyErrors.None)
        {
            Debug.Write("ssl errors: "+sslPolicyErrors.ToString());
        }

        return true;
    }

    protected static async Task Connect()
    {
        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;

            Config = SocketConfig.ReadConfig();

            if (Config is null) throw new FileNotFoundException("SOCKET_CONFIG_READ_EXCEPTION");

            Client = new TcpClient();
            
            await Client.ConnectAsync(Config.Ip,Config.Port);

            Stream = new SslStream(Client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));

            await Stream.AuthenticateAsClientAsync(Config.Ip);

            if (Client.Connected && Stream.IsAuthenticated)
            {
                foreach (var action in OnConnectionActions)
                {
                    action.Invoke();
                }
            }
        }

        catch (Exception ex) /* Logging this exception can expose ip and port to server */
        {
            Debug.Write(ex); 
        }
    }

    public static bool AbleToSend()
    {
        if (Client is null || Stream is null || !Stream.IsAuthenticated|| !Client.Connected)
        {
            Logger.Push($"Stream: {Stream is null} Client: {Client is null} Connected? {Client?.Connected} Auth: {Stream?.IsAuthenticated}", LogLevel.Error);
            return false;
        }
        return true;
    }
}
