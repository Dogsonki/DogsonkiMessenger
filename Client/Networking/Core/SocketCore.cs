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

    private static void ProcessBuffer(string buffer)
    {
        SocketPacket packet;
        if (!SocketPacket.TryDeserialize(out packet, buffer))
            return;
        foreach (RequestedCallback callback in RequestedCallback.Callbacks)
        {
            if (packet.Token == callback.GetToken())
            {
                callback.Invoke(packet.Data);
                return;
            }
        }
        ReadRawBuffer(packet);
    }

    private static void Recive()
    {
        byte[] buffer = new byte[MaxBuffer];
        int LenBuffer;

        while (IsConnected)
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
                        indexDollar = LongBuffer.IndexOf('$'); //Finding dollar is really expensive when is long 
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
                else
                {
                    RedirectConnectionLost(ex);
                }
            }
            Thread.Sleep(10);
        }
    }

    private static void RedirectConnectionLost(Exception ex, [CallerLineNumber] int lineNumber = 0)
    {
        Debug.Error(lineNumber + "::" + ex);
        MainThread.BeginInvokeOnMainThread(() => StaticNavigator.PopAndPush(new LoginPage()));
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
                        if (packet == null)//TODO: crashing on really small packet that trying to ddos server
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
            SocketQueue.Renew();
            Thread.Sleep(10);
        }
    }

    private static void ReadRawBuffer(SocketPacket packet) => Tokens.Process(packet);

    public static bool SendR(Action<object> Callback, object SendingData, Token token)
    {
        if (!AbleToSend())
            return false;

        RequestedCallback.AddCallback(new RequestedCallback(Callback, SendingData, (int)token));
        Send(SendingData, token);
        return true;
    }

    public static bool Send(object data, Token token = Token.EMPTY, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
    {
        if (!AbleToSend())
            return false;
        SocketPacket model = new SocketPacket(data, token);
        SocketQueue.Add(model);

        return true;
    }

    public static bool SendPacket(SocketPacket packet)
    {
        if (!AbleToSend())
            return false;
        SocketQueue.Add(packet);
        return true;
    }
}