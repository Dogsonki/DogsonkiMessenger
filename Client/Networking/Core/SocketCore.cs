using Client.Commands;
using Client.Networking.Model;
using Client.Utility;
using System.Text;

namespace Client.Networking.Core;

public class SocketCore : Connection
{
    public static async void Start() 
    {
        await Connect().ContinueWith((_) =>
        {
            if (!AbleToSend()) return;
            Task.Run(async () => await Receive());
            Task.Run(async () => await SendQueue());
        });  
    }

    private static void ReadRawBuffer(SocketPacket packet) => Tokens.Process(packet);


    private static string LongBuffer = "";

    private static void ProcessBuffer(string buffer)
    {
        SocketPacket packet = null;
        try
        {
            if (!SocketPacket.TryDeserialize(out packet, buffer))
                return;
        }
        catch (Exception ex)
        {
            Logger.Push("Buffer: " + buffer + " ERROR", TraceType.Packet, LogLevel.Error);
            Logger.Push(ex, TraceType.Packet, LogLevel.Error);
        }

        if (packet is null) return;

        if (packet.Token == (int)Token.CHAT_MESSAGE)
        {
            ReadRawBuffer(packet);
            return;
        }

        ThreadPool.QueueUserWorkItem((_) =>
        {
            if (!RequestedCallback.InvokeCallback(packet.Token, Convert.ToString(packet.Data))) return;

            ReadRawBuffer(packet);
        });
    }

    private static async Task Receive()
    {
        byte[] buffer = new byte[MAX_BUFFER_SIZE];
        int LenBuffer;

        while (true)
        {
            if (!AbleToSend() || Stream is null) continue;
            try
            {
                while ((LenBuffer = await Stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string DecodedString = Encoding.UTF8.GetString(buffer, 0, LenBuffer);

                    if (string.IsNullOrEmpty(DecodedString))
                        continue;

                    LongBuffer += DecodedString;

                    string buff;
                    int indexDollar = 1;

                    while (indexDollar > 0)
                    {
                        indexDollar = LongBuffer.IndexOf('$', StringComparison.Ordinal);

                        if (indexDollar == -1)
                            break;

                        buff = LongBuffer.Substring(0, indexDollar);

                        Logger.Push(buff, TraceType.Packet, LogLevel.Debug);

                        LongBuffer = LongBuffer.Substring(indexDollar + 1);

                        ProcessBuffer (buff);
                    }

                    Stream.Flush();
                    buffer = new byte[MAX_BUFFER_SIZE];
                }
            }
            catch (Exception ex)
            {
                Logger.Push(ex, TraceType.Packet, LogLevel.Error);
                if (ex is InvalidCastException)
                {
                    Debug.Error("Error when casting buffer into packet " + ex);
                }
                else if (Stream == null)
                {
                    await Connect();
                    Debug.Error("Connection stream is null");
                }
            }
            Thread.Sleep(5);
        }
    }

    private static async Task SendQueue()
    {
        while (true)
        {
            if (Stream.CanWrite && SocketQueue.CanSend())
            {
                SocketQueue.IsSending = true;
                SocketPacket packet = SocketQueue.Queue.Dequeue();

                try
                {
                    if (packet is null) throw new Exception("SEND_PACKET_NULL");

                    if (!AbleToSend()) return;
                    byte[] buffer = packet.GetPacked();

                    await Stream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Logger.Push(ex, TraceType.Packet, LogLevel.Error);
                }

                SocketQueue.IsSending = false;
            }
            Thread.Sleep(5);
        }

    }

    public static bool SendCallback(Action<object> callback, object sendingData, Token token, bool sendAbleOnce = true)
    {
        if (!AbleToSend()) return false;

        if (sendAbleOnce && RequestedCallback.IsAlreadyQueued(token))
        {
            Debug.Write($"Token AlreadyQueued {token}");
            return true;
        }

        RequestedCallback.AddCallback(new RequestedCallbackModel(callback, (int)token));

        Send(sendingData, token);

        return true;
    }

    public static bool Send(object data, Token token = Token.EMPTY, bool sendAbleOnce = false)
    {
        if (!AbleToSend()) return false;

        if (token == Token.EMPTY)
            return true;

        if (sendAbleOnce && RequestedCallback.IsAlreadyQueued(token))
        {
            Debug.Write($"Token AlreadyQueued {token}");
            return true;
        }

        Task.Run(async () =>
        {
            SocketPacket packet = new SocketPacket(data, token);
            SocketQueue.Add(packet);
        });

        return true;
    }
    
    public static bool SendCommand(ICommand command)
    {
        if (!AbleToSend()) return false;

        Task.Run(async () =>
        {
            SocketPacket packet = new SocketPacket(command, Token.BOT_COMMAND);
            SocketQueue.Add(packet);
        });

        return true;
    }
}