using Client.Networking.Models;
using Client.Utility;
using System.Text;
using Client.Networking.Commands;

namespace Client.Networking.Core;

public class SocketCore : Connection
{
    private static SocketCore instance { get; } = new SocketCore();

    private static readonly Dictionary<Token, Action<SocketPacket>> OnTokenReceived = new Dictionary<Token, Action<SocketPacket>>();

    private string LongBuffer = string.Empty;

    public SocketCore() : base()
    {

    }

    /// <summary>
    /// Main function to start connection
    /// </summary>
    public static async void Start()
    {
        if (await instance.Connect())
        {
            var ReceiveTask = Task.Run(instance.Receive);
            var SendQueueTask = Task.Run(instance.SendQueue);

            await Task.WhenAll(ReceiveTask, SendQueueTask);
        }
    }

    private void ProcessBuffer(string buffer)
    {
        ThreadPool.QueueUserWorkItem((_) =>
        {
            SocketPacket? packet = null;

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
            
            Token token = (Token)packet.PacketToken;

            if (OnTokenReceived.ContainsKey(token))
            {
                OnTokenReceived[token].Invoke(packet);
                return;
            }

            RequestedCallback.InvokeCallback(packet.PacketToken, packet);
        });
    }

    private async Task Receive()
    {
        byte[] buffer = new byte[MAX_BUFFER_SIZE];
        int LenBuffer;

        while (AbleToSend())
        {
            try
            {
                while ((LenBuffer = await ConnectionStream!.ReadAsync(buffer, 0, buffer.Length)) != 0)
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

                    ConnectionStream.Flush();
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
                else if (ConnectionStream == null)
                {
                    Debug.Error("Connection stream is null");
                }
            }
            await Task.Delay(3);
        }
    }

    private async Task SendQueue()
    {
        while (true)
        {
            if (ConnectionStream!.CanWrite && SocketQueue.CanSend() && AbleToSend())
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
#if DEBUG
                    Debug.Write($"Socket Sending: token: {packet.PacketToken} buffer_length:{buffer.Length} data: {packet.Serialize()}", false);
#endif
                    await ConnectionStream.WriteAsync(buffer, 0, buffer.Length);
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

    public static bool SendCallback(object sendingData, Token token, Action<SocketPacket> callback, bool waitForResponse = true)
    {
        if (!instance.AbleToSend()) return false;

        if (waitForResponse && RequestedCallback.IsAlreadyQueued(token))
        {
            return true;
        }

        RequestedCallback.AddCallback(new RequestedCallbackModel(callback, (int)token));

        Send(sendingData, token);

        return true;
    }

    public static void SetGlobalOnToken(Token token, Action<SocketPacket> callback)
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

    public static void OnConnectionConnected(Action<bool> action) => instance.AddOnConnection(action);

    public static bool Send(object data, Token token = Token.EMPTY, bool waitForResponse = false)
    {
        if (!instance.AbleToSend()) return false;

        if (token == Token.EMPTY)
            return true;

        if (waitForResponse && RequestedCallback.IsAlreadyQueued(token))
        {
            return true;
        }

        SocketPacket packet = new SocketPacket(data, token);
        SocketQueue.Add(packet);

        return true;
    }

    public static bool SendCommand(ICommand command)
    {
        if (!instance.AbleToSend()) return false;

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