using Client.Networking.Model;
using Client.Pages;
using System.Runtime.CompilerServices;
using System.Text;

namespace Client.Networking.Core;

public class SocketCore : Connection
{
    public SocketCore() : base(Recive, MenageQueue) { }

    public static void Init() => new SocketCore();

    private static string LongBuffer = "";
    private static readonly object ProcessPadLock = new object();

    private static void ProcessBuffer(string buffer)
    {
        SocketPacket packet;
        if (!SocketPacket.TryDeserialize(out packet, buffer))
            return;

        Debug.Write(packet.Data + " TOKEN: " +packet.Token);

        if (packet.Token == (int)Token.CHAT_MESSAGE)
        {
            ReadRawBuffer(packet);
            return;
        }
        
        ThreadPool.QueueUserWorkItem((object stateInfo) =>
        {
            foreach (RequestedCallback callback in RequestedCallback.Callbacks)
            {
                if (packet.Token == callback.GetToken())
                {
                    callback.Invoke(packet.Data);
                    return;
                }
            }
            ReadRawBuffer(packet);
        });
    }

    private static void Recive()
    {
        byte[] buffer = new byte[MaxBuffer];
        int LenBuffer;

        while (true)
        {
            try
            {
                while ((LenBuffer = Stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string DecodedString = Encoding.UTF8.GetString(buffer, 0, LenBuffer);

                    if (string.IsNullOrEmpty(DecodedString))
                        continue;

                    LongBuffer += DecodedString;
                    string buff;
                    int indexDollar = 1;

                    while (indexDollar > 0)
                    {
                        indexDollar = LongBuffer.IndexOf('$'); //Finding dollar is really expensive when buffer is long 
                                                               //buffer try rework this
                        if (indexDollar == -1)
                            break;

                        buff = LongBuffer.Substring(0, indexDollar);
                        LongBuffer = LongBuffer.Substring(indexDollar + 1);
                        ProcessBuffer(buff);
                    }
                    Stream.Flush();
                    buffer = new byte[MaxBuffer];
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidCastException)
                {
                    Debug.Error("Error in casting buffer into packet " + ex);
                }
                else if(Stream == null)
                {
                    Connect();
                    Debug.Error("Connection stream is null");
                }
                else
                {
                    Debug.Error(ex);
                    //RedirectConnectionLost(ex);
                }
            }
            Thread.Sleep(10);
        }
    }

    private static void RedirectConnectionLost(Exception ex, [CallerLineNumber] int lineNumber = 0)
    {
        Debug.Error(lineNumber + "::" + ex);
        MainThread.BeginInvokeOnMainThread(() => StaticNavigator.PopAndPush(new LoginPage("Connection with server lost")));
    }

    private static void MenageQueue()
    {
        while (true)
        {
            if (SocketQueue.AbleToSend())
            {
                foreach (SocketPacket packet in SocketQueue.GetSendingPackets)
                {
                    byte[] Buffer = new byte[MaxBuffer];
                    try
                    {
                        if (packet == null)
                            continue;

                        Buffer = packet.GetPacked();
                        Stream.Write(Buffer, 0, Buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        Debug.Error($"Exception when sending buffer Length: {Buffer?.Length}");
                        RedirectConnectionLost(ex);
                    }
                }
            }
            if (Client != null && Stream != null && IsConnected)
            {
               SocketQueue.Renew();
            }
           
            Thread.Sleep(5);
        }
    }

    private static void ReadRawBuffer(SocketPacket packet) => Tokens.Process(packet);

    public static bool SendCallback(Action<object> Callback, object SendingData, Token token)
    {
        if (!AbleToSend())
            return false;
        ThreadPool.QueueUserWorkItem(delegate
        {
            RequestedCallback.AddCallback(new RequestedCallback(Callback, SendingData, (int)token));
            Send(SendingData, token);
        }, null);
       
        return true;
    }

    public static bool Send(object data, Token token = Token.EMPTY)
    {
        if (!AbleToSend())
            return false;

        ThreadPool.QueueUserWorkItem(delegate
        {
            SocketPacket model = new SocketPacket(data, token);
            SocketQueue.Add(model);

        }, null);

        return true;
    }

    public static bool SendPacket(SocketPacket packet)
    {
        if (!AbleToSend())
            return false;
        ThreadPool.QueueUserWorkItem(delegate
        {
            SocketQueue.Add(packet);
        }, null);

        return true;
    }
}