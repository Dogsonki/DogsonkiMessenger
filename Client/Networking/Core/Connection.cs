using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Client.Utility;
using Client.Networking.Models;

namespace Client.Networking.Core;

public class Connection
{
    private TcpClient ConnectionClient { get; }
    protected SocketConfig Config { get; } = new SocketConfig();

    protected SslStream? ConnectionStream { get; set; }

    protected static bool IsConnected { get; private set; }
    protected static bool IsInitialized { get; private set; } = false;

    protected const int MAX_BUFFER_SIZE = 1024 * 48;

    protected static bool IsConnecting { get; set; }

    private static readonly List<Action<bool>> OnConnectionActions = new List<Action<bool>>();

    public Connection()
    {
        Config.ReadConfig();

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;

        if (Config is null) throw new FileNotFoundException("SOCKET_CONFIG_READ_EXCEPTION");

        ConnectionClient = new TcpClient()
        {
            NoDelay = true,
            ReceiveBufferSize = 1024 * 8,
            SendBufferSize = MAX_BUFFER_SIZE
        };
    }

    /// <summary>
    /// Invokes {action} when client try to connect
    /// </summary>
    /// <param name="action">If connected</param>
    public void AddOnConnection(Action<bool> action)
    {
        OnConnectionActions.Add(action);

        if (AbleToSend())
        {
            action.Invoke(true);    
        }
    }
    
    public async Task<bool> Connect()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;

        if (Config is null) throw new FileNotFoundException("SOCKET_CONFIG_READ_EXCEPTION");

        ConnectionClient.Connect(Config.Ip, Config.Port);

        ConnectionStream = new SslStream(ConnectionClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));

        await ConnectionStream.AuthenticateAsClientAsync(Config.Ip);

        if (ConnectionClient.Connected && ConnectionStream.IsAuthenticated)
        {
            foreach (var action in OnConnectionActions)
            {
                action.Invoke(true);
            }
        }

        return ConnectionClient.Connected;
    }

    private static void InvokeOnConnectionActions(bool status)
    {
        foreach (var action in OnConnectionActions)
        {
            action.Invoke(status);
        }
    }

    public static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        if(sslPolicyErrors != SslPolicyErrors.None)
        {
            Debug.Write("ssl errors: "+sslPolicyErrors.ToString());
        }

        return true;
    }

    public bool AbleToSend()
    {
        if(ConnectionStream is null || ConnectionClient is null) return false;

        return ConnectionClient is not null || ConnectionStream is not null || ConnectionStream.IsAuthenticated || ConnectionClient.Connected;
    }
}
