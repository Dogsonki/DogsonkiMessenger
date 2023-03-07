using Client.Networking.Models;
using Client.Utility;
using System.Text;
using Client.Networking.Commands;

namespace Client.Networking.Core;

/* TODO:
    Use StringBuilder as buffer
    Find end of packet from last end of buffer
*/

public class SocketCore : Connection
{
    private static readonly Dictionary<Token, Action<SocketPacket>> OnTokenReceived = new Dictionary<Token, Action<SocketPacket>>();

    /// <summary>
    /// Main function to start connection
    /// </summary>
    public static async void Start()
    {
        await Connect().ContinueWith((_) =>
        {
            if (!AbleToSend()) return;

            var ReceiveTask = Task.Run(Receive);
            var SendQueueTask = Task.Run(SendQueue);

            Task.WhenAll(ReceiveTask, SendQueueTask);
        });
    }

    /* Change MAX_BUFFER_SIZE to max accepted buffer by device*/
    //private static StringBuilder LongBuffer = new StringBuilder(MAX_BUFFER_SIZE);

    private static string LongBuffer = string.Empty;

    private static void ProcessBuffer(string buffer)
    {
        ThreadPool.QueueUserWorkItem((_) =>
        {
            SocketPacket packet = null;
            try
            {
                if (!SocketPacket.TryDeserialize(out packet, buffer))
                    return;
            }
            catch (Exception ex)
            {
                Logger.Push(ex, LogLevel.Error);
            }

            if (packet is null) return;

            if (OnTokenReceived.ContainsKey((Token)packet.PacketToken))
            {
                OnTokenReceived[(Token)packet.PacketToken].Invoke(packet);
                return;
            }

            RequestedCallback.InvokeCallback(packet.PacketToken, packet);
        });
    }

    private static async Task Receive()
    {
        byte[] buffer = new byte[MAX_BUFFER_SIZE];
        int LenBuffer;

        while (true)
        {
            if (!AbleToSend()) continue;
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
#if DEBUG
                        Debug.Write($"Last buffer: {buff}", printPath: false);
#endif
                        LongBuffer = LongBuffer.Substring(indexDollar + 1);

                        ProcessBuffer(buff);
                    }

                    Stream.Flush();
                    buffer = new byte[MAX_BUFFER_SIZE];
                }
            }
            catch (Exception ex)
            {
                Logger.Push(ex, LogLevel.Error);

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
            await Task.Delay(3);
        }
    }

    private static async Task SendQueue()
    {
        while (true)
        {
            if (Stream.CanWrite && SocketQueue.CanSend() && AbleToSend())
            {
                SocketQueue.IsSending = true;
                SocketPacket packet = SocketQueue.Queue.Dequeue();

                try
                {
                    if (packet is null)
                    {
#if DEBUG
                        throw new Exception($"SEND_PACKET_NULL");
#else
                        Logger.PushException(new Exception("SEND_PACKET_NULL"));
#endif
                    }

                    byte[] buffer = packet.GetPacked();

                    Debug.Write($"Socket Sending: {buffer.Length} UnPacked: {packet.Data}", false);

                    await Stream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Logger.Push(ex, LogLevel.Error, TraceType.Packet);
                }

                SocketQueue.IsSending = false;
            }
            await Task.Delay(3);
        }
    }

    public static bool SendCallback(object sendingData, Token token, Action<SocketPacket> callback, bool sendAbleOnce = true)
    {
        if (!AbleToSend()) return false;

        if (sendAbleOnce && RequestedCallback.IsAlreadyQueued(token))
        {
            return true;
        }

        RequestedCallback.AddCallback(new RequestedCallbackModel(callback, (int)token));

        Send(sendingData, token);

        return true;
    }

    public static void SetGlobalOnToken(Token token, Action<object> callback)
    {
        if (!OnTokenReceived.ContainsKey(token))
        {
            OnTokenReceived.Add(token, callback);
        }
        else
        {
            throw new Exception($"Tried to add global token twice token:{token} callback: {callback.Method.Name}");
        }
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

        SocketPacket packet = new SocketPacket(data, token);
        SocketQueue.Add(packet);

        return true;
    }

    public static bool SendCommand(ICommand command)
    {
        if (!AbleToSend()) return false;

        SocketPacket packet = new SocketPacket(command, Token.BOT_COMMAND);
        SocketQueue.Add(packet);

        return true;
    }

    /// <summary>
    /// Invokes function when client received specific token from server
    /// </summary>
    public static void OnToken(Token token, Action<SocketPacket> callback)
    {
        if (OnTokenReceived.ContainsKey(token))
        {
            OnTokenReceived[token] = callback;
        }
        else
        {
            OnTokenReceived.Add(token, callback);
        }
    }
}