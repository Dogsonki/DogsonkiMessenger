using Client.IO;
using Client.Model.Session;
using Client.Models;
using Client.Networking.Config;
using Client.Networking.Model;
using Client.Pages;
using Client.Utility;
using Client.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace Client.Networking
{
    public enum EncodingOption
    {
        UFT8,
        BASE64,
        IMAGE
    }

    public class SocketCore
    {
        protected static TcpClient Client { get; set; }
        protected static NetworkStream Stream { get; set; }
        protected static SocketConfig Config { get; set; }
        protected static Thread ReciveThread { get; set; }
        protected static Thread ManageSendingQueue { get; set; }
        protected static bool IsConnected { get; set; }
        protected static EncodingOption ActualDecoding { get; set; }
        protected static List<byte> m_ReadingImage = new List<byte>();
        protected static bool Initialized = false;
        protected static bool PendingConnection = false;
        private const int MaxBuffer = 1024;
        protected static bool WillReadImage { get; set; }

        private static Thread SocketInitTemporaryThread;

        public static bool Init()
        {
            Debug.Write("Initializating sockets");
            Config = SocketConfig.ReadConfig();
            Debug.Write("Readed socket config");
            if(SocketInitTemporaryThread == null)
            {
                SocketInitTemporaryThread = new Thread(() =>
                {
                    try
                    {
                        Client = new TcpClient(Config.Ip, Config.Port);
                        if (Client != null)
                        {
                            Stream = Client.GetStream();
                        }
                    }
                    catch (Exception ex) { Debug.Error(ex); }

                  
                    //Thread should be interputed
                });
                SocketInitTemporaryThread.Start();
            }
            try
            {
              
            }
            catch (Exception ex)
            {
                Debug.Error("Unable connect: " + ex);
                return false;
            }

            if (Client == null)
            {
                Debug.Error("Unable connect, client was null");
                return false;
            }

            Debug.Write("Starting socket threads");

            Client.ReceiveBufferSize = MaxBuffer;
            Client.SendBufferSize = MaxBuffer;
            IsConnected = true;

            ReciveThread = new Thread(Recive);
            ManageSendingQueue = new Thread(MenageQueue);

            ReciveThread.Start();
            ManageSendingQueue.Start();
            return true;
        }

        /// <summary>
        /// Will try to connect to server again if is not connected
        /// </summary>
        /// <returns>IsConnected</returns>
        public static bool TryConnect()
        {
            //TODO: Check if PendingConnection won't break connection
            PendingConnection = true;
            if (IsConnected)
                return true;

            if (!Initialized)
            {
                return Init();
            }

            if (PendingConnection)
                return false;

            try
            {
                Client = new TcpClient(Config.Ip, Config.Port);
                Stream = Client.GetStream();
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Unable connect: " + ex);
                return false;
            }
            PendingConnection = false;
            if (Client != null)
            {
                IsConnected = true;
                return true;
            }
            return false;
        }

        private static bool m_ReadNextImage = false;

        protected static void Recive()
        {
            byte[] buffer = new byte[MaxBuffer];
            int LenBuffer;

            try
            {
                while (IsConnected)
                {
                    try
                    {
                        while ((LenBuffer = Stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            bool _WillReadRaw = true;

                            string DecodedString = string.Empty;
                            switch (ActualDecoding)
                            {
                                case EncodingOption.IMAGE:
                                    int len = buffer.Length;
                                    bool _end = false;
                                    if (Encoding.UTF8.GetString(buffer,0,LenBuffer)[LenBuffer-1] == '$')
                                    {
                                        Debug.Write("ENDING READGING IMAGE");
                                        Debug.Write(Encoding.UTF8.GetString(buffer));
                                        len--;
                                        _end = true;
                                    }
                                 
                                    for(int i = 0; i < len; i++)
                                    {
                                        m_ReadingImage.Add(buffer[i]);
                                    }
                                    if (_end)
                                    {
                                        var i =JsonConvert.DeserializeObject<SocketPacket>(Encoding.UTF8.GetString(m_ReadingImage.ToArray()).Replace("$",""));
                                        Debug.Write("ENDED " + i.Token);
                                    }
                                    break;
;
                                case EncodingOption.UFT8:
                                    DecodedString = Encoding.UTF8.GetString(buffer, 0, LenBuffer);
                                    break;

                                case EncodingOption.BASE64:
                                    DecodedString = Encoding.UTF8.GetString(buffer, 0, LenBuffer);
                                    ActualDecoding = EncodingOption.UFT8;
                                    break;
                            }

                            if (WillReadImage)
                                continue;

                            string[] Buffers = DecodedString.Split('$');
                            for (int i = 0; i < Buffers.Length; i++)
                            {
                                if (WillReadImage)
                                {
                                    byte[] r = Encoding.UTF8.GetBytes(Buffers[i]);
                                    for (int y = 0; y < Buffers[i].Length; y++)
                                        m_ReadingImage.Add(r[y]);
                                }
                                 
                                _WillReadRaw = true;
                                if (Buffers[i].Length == 0)
                                    continue;
                                SocketPacket packet;
                                try
                                {
                                    packet = JsonConvert.DeserializeObject<SocketPacket>(Buffers[i]);
                                    Debug.Write(packet.Token + " | " + packet.Data.GetType());
                                }
                                catch (Exception ex)
                                {
                                    Debug.Write("[SOCKET RECIVE] IMAGE PASRING ");

                                    WillReadImage = true;
                                    ActualDecoding = EncodingOption.IMAGE;
                                    Debug.Write(Buffers[i]);
                                    Debug.Write("LAST: " + Buffers[i][Buffers.Length] +"PRE "+ Buffers[i][Buffers.Length-1]);
                                    byte[] bf = Encoding.UTF8.GetBytes(Buffers[i]);
                                    for(int y=0;y< bf.Length; y++)
                                    {
                                        m_ReadingImage.Add(bf[y]);
                                    }
                                    Debug.Write(Encoding.UTF8.GetString(m_ReadingImage.ToArray()));
                                    continue;
                                }
                                for (int j = 0; j < RequestedCallback.GetCount(); j++)
                                {
                                    RequestedCallback req = RequestedCallback.Callbacks[i];
                                    if (packet.Token == req.GetToken())
                                    {
                                        req.Invoke(packet.Data);
                                        _WillReadRaw = false;
                                    }
                                }

                                if (_WillReadRaw)
                                {
                                    ReadRawBuffer(packet);
                                }

                            }
                            Stream.Flush();
                            buffer = new byte[MaxBuffer];
                            DecodedString = string.Empty;
                        }
                    }
                    catch(Exception ex)
                    {
                        Debug.Write(ex);
                    }
                }

                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                RedirectConnectionLost(ex);

                //BUG: sametimes it doens't work for same reason 
                //TODO: set page to appentry when disconnected 
            }
        }

        protected static void RedirectConnectionLost(Exception ex)
        {
            Debug.Error(ex);
            Device.BeginInvokeOnMainThread(() =>
            {
                StaticNavigator.PopAndPush(new AppEntry());
            });
        }
        
        protected static void MenageQueue()
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
                            Debug.Write($"[SOCKET SENDING] {Buffer.Length} | {Encoding.UTF8.GetString(Buffer)}");
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
                Thread.Sleep(100);
            }
        }

        private static void ReadRawBuffer(SocketPacket packet)
        {
            Debug.Write($"Reading raw buffer {packet.Token}");
            switch (packet.Token)
            {
                case -1:
                    Debug.Write("Samething went wrong");
                    break;
                case 4:
                    PeopleFinder.ParseQuery(packet.Data);
                    break;
                case 5:
                    try
                    {
                        Device.InvokeOnMainThreadAsync(() => MessageViewModel.AddMessage(((JObject)packet.Data).ToObject<MessageModel>()));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cannot parse MessageModel {ex}");
                    }
                    break;
                case 6:
                    MainAfterLoginViewModel.ParseQuery((JArray)packet.Data);
                    break;
                case 7:
                    if (m_ReadNextImage)
                    {
                    }
                    else
                    {
                    }
                    break;
                case 9:
                    Session session = ((JObject)packet.Data).ToObject<Session>();
                    StorageIO.Write(session, "session");
                    break;
                case 10:
                    if (((string)packet.Data).Length > 0)
                    {
                        LocalUser.Username = (string)packet.Data;
                        Device.InvokeOnMainThreadAsync(() => StaticNavigator.PopAndPush(new MainAfterLoginPage()));
                    }
                    break;
                case 11:
                    Debug.Write("READING IMAGE ...");
                    WillReadImage = true;
                    ActualDecoding = EncodingOption.IMAGE;
                    break;
                default:
                    Console.WriteLine($"Cannot find raw definition");
                    break;
            }
        }

        public static bool SendFile(byte[] fileinbytes)
        {
            /* 
             Buffer cannot be encoded into UTF8 or BASE64 
             have to be send just a raw byte[] 
            */
            if (!AbleToSend())
                return false;

            SocketPacket packet = new SocketPacket(fileinbytes, 8);
            SlicedBuffer sb = new SlicedBuffer(packet.GetPacked(), MaxBuffer);
            List<byte[]> sliced = sb.GetSlicedBuffer();

            foreach(var a in sliced)
            {
                Stream.Write(a);
            }
            return true;
        }

        public static bool SendR(Action<object> Callback, object SendingData, int Token)
        {
            if (!AbleToSend())
                return false;

            RequestedCallback.Callbacks.Add(new RequestedCallback(Callback, SendingData, Token));
            Send(SendingData, Token);
            return true;
        }

        private static bool AbleToSend()
        {
            if (Client == null || Stream == null)
            {
                Debug.Error("Client is null");
                return false;
            }

            if (!IsConnected)
            {
                Debug.Error("Client is not connected to server");
                return false;
            }

            if (!Client.Connected)
            {
                Debug.Error("Client disconnected form server");
                return false;
            }
            return true;
        }

        public static bool Send(object data, int token = -1, EncodingOption option = EncodingOption.UFT8, bool isImage = false, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
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
}