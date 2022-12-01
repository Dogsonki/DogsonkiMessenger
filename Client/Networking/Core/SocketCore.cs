﻿using Client.Networking.Commands;
using Client.Networking.Models;
using Client.Utility;
using System.Text;

namespace Client.Networking.Core;

public class SocketCore : Connection
{
    private static readonly Dictionary<Token, Action<object>> OnTokenReceived = new Dictionary<Token, Action<object>>();

    public static async void Start()
    {
        await Connect().ContinueWith((_) =>
        {
            if (!AbleToSend()) return;
            Task.Run(async () => await Receive());
            Task.Run(async () => await SendQueue());
        });
    }

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

            //Don't convert packet.Data to JObject
            string? packetData = Convert.ToString(packet.Data);

            if(packetData is null)
            {
                throw new InvalidCastException($"PacketData is not a string: {packet.Data}");
            }

            if (OnTokenReceived.ContainsKey((Token)packet.PacketToken))
            {
                OnTokenReceived[(Token)packet.PacketToken].Invoke(packetData);
                return;
            }

            RequestedCallback.InvokeCallback(packet.PacketToken, packetData);
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

                        /* Visual studio have rate limit in console buffer, images longer than 1mb will crash your visual studio */
#if DEBUG
                        Logger.Push(buff, LogLevel.Debug, TraceType.Packet);
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
            Thread.Sleep(5);
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
                    if (packet is null) throw new Exception("SEND_PACKET_NULL");

                    Debug.Write($"Sending: {packet.Data} {packet.PacketToken}");

                    byte[] buffer = packet.GetPacked();

                    Debug.Write(Encoding.UTF8.GetString(buffer));

                    await Stream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Logger.Push(ex, LogLevel.Error, TraceType.Packet);
                }

                SocketQueue.IsSending = false;
            }
            Thread.Sleep(3);
        }
    }

    public static bool SendCallback(object sendingData, Token token, Action<object> callback, bool sendAbleOnce = true)
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
    public static void OnToken(Token token, Action<object> callback)
    {
        if (OnTokenReceived.ContainsKey(token))
        {
            Logger.Push("Changing action on token received", LogLevel.Warning);
            OnTokenReceived[token] = callback;
        }
        else
        {
            OnTokenReceived.Add(token, callback);
        }
    }
}