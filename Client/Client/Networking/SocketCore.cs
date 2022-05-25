using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Client.Utility;
using Newtonsoft.Json;
using Client.Pages;
using Client.Views;
using Client.Models;
using Client.Networking.Config;
using System.IO;
using Client.Networking.Model;
using Xamarin.Forms;
using Client.IO;
using Newtonsoft.Json.Linq;
using Client.Model.Session;

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
        protected static List<byte[]> m_ReadingImage = new List<byte[]>();
        protected static bool Initialized = false;

        private const int MaxBuffer = 1024;

        public static bool Init()
        {
            Config = SocketConfig.ReadConfig();

            try
            {
                Client = new TcpClient(Config.Ip, Config.Port);
                Stream = Client.GetStream();
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
            if (IsConnected)
                return true;

            if (!Initialized)
            {
                return Init();
            }

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
            if (Client != null)
            {
                IsConnected = true;
                return true;
            }
            return false;
        }

        private static bool m_ReadNextImage = false;   

        private static void Recive()
        {
            byte[] buffer = new byte[MaxBuffer];
            int LenBuffer;

            try
            {
                while (IsConnected)
                {
                    while ((LenBuffer = Stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        bool _WillReadRaw = true;

                        string DecodedString = string.Empty;
                        switch (ActualDecoding)
                        {
                            //When server starts sending image client have to decode this by BASE64 
                            //otherwise image will be corrupted
                            //Token 0007 

                            case EncodingOption.IMAGE:
                                m_ReadingImage.Add(buffer);
                                break;

                            case EncodingOption.UFT8:
                                DecodedString = Encoding.UTF8.GetString(buffer, 0,LenBuffer);
                                break;

                            case EncodingOption.BASE64:
                                DecodedString = Encoding.UTF8.GetString(buffer, 0, LenBuffer);
                                ActualDecoding = EncodingOption.UFT8;
                                break;
                        }
                        string[] Buffers = DecodedString.Split('$');
                        for(int i = 0; i < Buffers.Length; i++)
                        {
                            _WillReadRaw = true;
                            if (Buffers[i].Length == 0)
                                continue;
                            SocketPacket packet;
                            try
                            {
                                packet = JsonConvert.DeserializeObject<SocketPacket>(Buffers[i]);
                                Debug.Write(packet.Token + "" + packet.Data.GetType());
                            }
                            catch(Exception ex)
                            {
                                Debug.Error($"Cannot deserialize buffer to packet: {Buffers[i]} | {ex}");
                                continue;
                            }

                            for (int j= 0; j < RequestedCallback.GetCount(); j++)
                            {
                                RequestedCallback req = RequestedCallback.Callbacks[i];
                                if (packet.Token == req.GetToken())
                                {
                                    req.Invoke(packet.Data);
                                    _WillReadRaw=false;
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

                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                StaticNavigator.PopAndPush(new AppEntry());//TODO: set page to appentry when disconnected 
            }
        }

        private static void ReadRawBuffer(SocketPacket packet)
        {
            Debug.Write($"Reading raw buffer {packet.Token}");
            switch (packet.Token)
            {
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
                        Console.WriteLine("<Ending image>");
                        Console.WriteLine($"Image len: {m_ReadingImage[0].Length+m_ReadingImage[1].Length}");

                        List<byte> image = new List<byte>();
                        foreach(var a in m_ReadingImage)
                        {
                            Console.WriteLine("byte: " + $@"{Encoding.UTF8.GetString(a)}");
                            foreach (var r in a)
                            {
                                image.Add(r);
                            }
                        }
                        ImageSource d = ImageSource.FromStream(() => new MemoryStream(image.ToArray()));
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            MainAfterLoginPage.f.Source = d;
                        });
                        Console.WriteLine("is image readed? ");
                        ActualDecoding = EncodingOption.UFT8; 
                    }   
                    else
                    {
                        Console.WriteLine("<Starting image> ");
                        ActualDecoding = EncodingOption.IMAGE;
                        m_ReadNextImage = true;
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
                default:
                    Console.WriteLine($"Cannot find raw definition");
                    break;
            }
        }
    
        public static void SendFile(byte[] stream)
        {
            Console.WriteLine($"Len of sending image: {stream.Length}");
            ImageSource d = ImageSource.FromStream(() => new MemoryStream(stream)); 

            SlicedBuffer sb = new SlicedBuffer(stream, 1024);
        }

        public static void SendFile(string resourceName, string location = "temp")
        {
            /* 
             Buffer cannot be encoded into UTF8 or BASE64 
             have to be send just a raw byte[] 
             */
            IFileService sr = DependencyService.Get<IFileService>();
            byte[] buffer = sr.ReadFileFromStorage(resourceName, location);

            if (buffer.Length < 200)
                Console.WriteLine("Buffer was null");

            SlicedBuffer sb = new SlicedBuffer(buffer, MaxBuffer);
            SendRaw("3");

            sb.GetSlicedBuffer().ForEach((buff) =>
            {
                SendRaw(buff);
            });

            SendRaw("ENDFILE");
        }
         
        public static bool SendR(Action<object> Callback, object SendingData,int Token)
        {
            if (!AbleToSend())
                return false;
            RequestedCallback.Callbacks.Add(new RequestedCallback(Callback,SendingData,Token));
            SendRaw(SendingData,Token);
            return true;
        }

        public static void MenageQueue()
        {
            while (true)
            {
                if (SocketQueue.AbleToSend())
                {
                    foreach(SocketPacket packet in SocketQueue.GetSendingPackets)
                    {
                        byte[] Buffer = new byte[MaxBuffer];
                        try
                        {
                            Buffer = packet.GetPacked();
                            Console.WriteLine($"[SOCKET SENDING] {Buffer.Length} | {Encoding.UTF8.GetString(Buffer)}" );
                            Stream.Write(Buffer, 0, Buffer.Length);
                        }
                        catch (Exception arg)
                        {
                            Debug.Error($"Exception when sending buffer Length: {Buffer?.Length} IsImage: {arg}");
                        }
                    }
                }
                SocketQueue.Renew();
                Thread.Sleep(100);
            }
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

        public static bool SendRaw(object data, int token = -1, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
        {
            if (!AbleToSend())
                return false;
            AddToQueue(data,token);
            
            return true;
        }

        /// <summary>
        /// Packs package and preparing how to send it 
        /// </summary>
        /// <param name="data"></param>
        private static void AddToQueue(object data,int token=-1, EncodingOption option = EncodingOption.UFT8,bool isImage=false)
        {
            SocketPacket model = new SocketPacket(data, token);

            SocketQueue.Add(model);

            return;
            if (data.GetType() == typeof(byte[]))
            {
                byte[] _Data = (byte[])data;

                if (_Data.Length == 0)
                    return;
                model = new SocketPacket(data,token); 
            }
            else if(data.GetType() == typeof(string))
            {
                if (string.IsNullOrEmpty((string)data))
                    return;
                model = new SocketPacket(data,token);
            }
            else
            {
                Debug.Error("Cannot pack package => wrong data type");
            }
            SocketQueue.Add(model);
        }
    }
}