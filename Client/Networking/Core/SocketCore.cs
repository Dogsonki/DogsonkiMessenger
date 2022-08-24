using Client.Networking.Models.BotCommands;
using Client.Networking.Model;
using Client.Pages;
using Client.Utility;
using System.Runtime.CompilerServices;
using System.Text;
using Client.Networking.Models;

namespace Client.Networking.Core;

public class SocketCore : Connection
{
    public static async void Start() 
    {
        await Connect().ContinueWith(async (_) =>
        {
            if (!AbleToSend()) return;

            await Recive();
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

        ThreadPool.QueueUserWorkItem((object stateInfo) =>
        {
            if (!RequestedCallback<SocketPacket>.InvokeCallback(packet.Token, (string)packet.Data)) return;

            ReadRawBuffer(packet);
        });
    }

    private static async Task Recive()
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

    private static async Task Send(SocketPacket packet)
    {
        try
        {
            if (packet is null) throw new Exception("SEND_PACKET_NULL");

            if (!AbleToSend()) return;

            byte[] buffer = packet.GetPacked();
            await Stream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch(Exception ex)
        {
            Logger.Push(ex, TraceType.Packet, LogLevel.Error);
        }
    }

    public static bool SendCallback<T>(Action<T> Callback, object SendingData, Token token, bool SendableOnce = true)
    {
        if (!AbleToSend()) return false;
        Debug.Write("Sending callback");
        if (SendableOnce && RequestedCallback<T>.IsAlreadyQueued(token))
        {
            return true;
        }

        Debug.Write("adding ");
        RequestedCallback<T>.AddCallback(new RequestedCallbackModel<T>(Callback, (int)token));

        Send(SendingData, token);

        return true;
    }

    public static bool Send(object data, Token token = Token.EMPTY)
    {
        if (!AbleToSend()) return false;

        Task.Run(async () =>
        {
            SocketPacket packet = new SocketPacket(data, token);
            await Send(packet);
        });

        return true;
    }

    public static bool SendCommand(IBotCommand command)
    {
        if (!AbleToSend()) return false;

        Task.Run(async () =>
        {
            SocketPacket packet = new SocketPacket(command, Token.BOT_COMMAND);
            await Send(packet);
        });

        return true;
    }
}