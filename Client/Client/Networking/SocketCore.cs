using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Client.Utility;
using Newtonsoft.Json;
using Client.Pages;
using System.Linq;
using Client.Views;
using Client.Models;
using System.Threading.Tasks;
using Client.Networking.Config;
using System.IO;
using Xamarin.Forms;
using Client.Utility.Services;

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

        private const int MaxBuffer = 1024 * 8;

        public static void Init()
        {
            Config = SocketConfig.ReadConfig();

            try
            {
                Client = new TcpClient(Config.Ip, Config.Port);
                Stream = Client.GetStream();
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Couldn't connect: " + ex);
            }

            Client.ReceiveBufferSize = MaxBuffer;
            Client.SendBufferSize = MaxBuffer;
            Client.NoDelay = false;
            IsConnected = true;
            ReciveThread = new Thread(Recive);
            ManageSendingQueue = new Thread(MenageQueue);

            ReciveThread.Start();
            ManageSendingQueue.Start();
        }   


        public static bool TryReconnect()
        {
            return true;
            try
            {
                Client = new TcpClient(Config.Ip, Config.Port);
                Stream = Client.GetStream();
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Couldn't connect: " + ex);
                return false;
            }
            return true;
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
                        Console.WriteLine($"[Socket Recived] {DecodedString}");

                        string[] Buffers = DecodedString.Split('$');
                        for(int i = 0; i < Buffers.Length; i++)
                        {
                            _WillReadRaw = true;
                            if (Buffers[i].Length == 0)
                                continue;
                            Console.WriteLine($"[Socket Decoded] {Buffers[i]}");
                            for(int j= 0; j < RequestedCallback.GetCount(); j++)
                            {
                                int tk;
                                RequestedCallback req = RequestedCallback.Callbacks[i];
                                if (GetToken(Buffers[i], out tk))
                                {
                                    if (tk == req.GetToken())
                                    {
                                        req.Invoke(RemoveToken(Buffers[i]));
                                        _WillReadRaw = false;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Cannot get token from recived message:" + Buffers[i]);
                                }
                                Console.WriteLine($"Token rev {tk} to {req.GetToken()}");
                                if (_WillReadRaw)
                                    ReadRawBuffer(Encoding.UTF8.GetString(buffer));

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
                StaticNavigator.PopAndPush(new AppEntry());
            }
        }

        private static void ReadRawBuffer(string RecivedMessage)
        {
            int token;
            if (!GetToken(RecivedMessage, out token))//TODO: Still can write sametimes to reading image
            {
                return;
            }

            switch (token)
            {
                case 0003:
                    Device.BeginInvokeOnMainThread(() => StaticNavigator.Push(new MessageView()));
                    break;
                case 0004:
                    PeopleFinder.ParseQuery(RemoveToken(RecivedMessage));
                    break;
                case 0005:
                    try
                    {
                        Console.WriteLine(RemoveToken(RecivedMessage));
                        Device.InvokeOnMainThreadAsync(()=>
                      
                            MessageViewModel.AddMessage(JsonConvert.DeserializeObject<MessageModel>(RemoveToken(RecivedMessage)))
                        );
                       
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine($"Cannot parse MessageModel {RecivedMessage} {ex}");
                    }
                    break;
                case 0006:
                    MainAfterLoginViewModel.ParseQuery(RemoveToken(RecivedMessage));    
                    break;
                case 0007:
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
                default:
                    Console.WriteLine($"Cannot find raw definition {RecivedMessage}");
                    break;
            }
        }
    
        private static string RemoveToken(string req)
        {
            if (req.Length < 5)
                return "";
            return req.Substring(5);
        }

        public static void SendFile(byte[] stream)
        {
            Console.WriteLine($"Len of sending image: {stream.Length}");
            ImageSource d = ImageSource.FromStream(() => new MemoryStream(stream)); 

            SlicedBuffer sb = new SlicedBuffer(stream, 1024);

            SendRaw("3");

            sb.GetSlicedBuffer().ForEach((buff) =>
            {
                SendRaw(buff);
            });
            SendRaw("ENDFILE");
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
         
        private static bool GetToken(string req, out int tk)
        {
            if (req.Length == 0)
            {
                tk = -1;
                return false;
            }

            string br = req.Substring(0, 4);
            if (int.TryParse(br.ToString(), out tk))
            {
                return true;
            }
            return false;
        }

        public static void SendR(Action<string> Callback, string SendingData,int Token)
        {
            RequestedCallback.Callbacks.Add(new RequestedCallback(Callback,SendingData,Token));
            SendRaw(SendingData);
        }

        public static void SendR(Task<string> callback, string data, int Token)
        {
            RequestedCallback.Callbacks.Add(new RequestedCallback(callback,data,Token));
            SendRaw(data);
        }

        protected static List<SocketMessageModel> SendingQueue = new List<SocketMessageModel>();
        protected static List<SocketMessageModel> UpdatedQueue = new List<SocketMessageModel>();

        public static void MenageQueue()
        {
            while (true)
            {
                if (SendingQueue.Count > 0)
                {
                    for (int i = 0; i < SendingQueue.Count; i++)
                    {
                        SocketMessageModel model = SendingQueue[i];
                        try
                        {
                            byte[] _buf;
                            try
                            {
                                _buf = model.Data;
                                Stream.Write(_buf, 0, _buf.Length);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Unregistred exception: " + ex);
                            }
                            _buf = null;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        SendingQueue.Remove(model);
                    }
                }
                if(UpdatedQueue.Count > 0)
                {
                    SendingQueue = new List<SocketMessageModel>(UpdatedQueue);
                    UpdatedQueue.Clear();
                }
                Thread.Sleep(100);
            }
        }

        public static bool SendRaw(object data, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
        {
            if (Client == null || Stream == null)
            {
                Console.WriteLine("Client is null");
                return false;
            }

            if (!IsConnected)
            {
                Console.WriteLine("Client is not connected to server");
                return false;
            }

            if (!Client.Connected)
            {
                Console.WriteLine("Client disconnected form server");
                return false;
            }

            AddToQueue(data);
            
            return true;
        }

        /// <summary>
        /// Packs package and preparing how to send it 
        /// </summary>
        /// <param name="data"></param>
        private static void AddToQueue(object data, EncodingOption option = EncodingOption.UFT8)
        {
            SocketMessageModel model = null;
            if (data.GetType() == typeof(byte[]))
            {
                byte[] _Data = (byte[])data;

                if (_Data.Length == 0)
                    return;

                model = new SocketMessageModel()
                {
                    isImage = true,
                    Data = (byte[])data
                };
                
            }
            else if(data.GetType() == typeof(string))
            {
                if (string.IsNullOrEmpty((string)data))
                    return;

                model = new SocketMessageModel();
                switch (option)
                {
                    case EncodingOption.UFT8:
                        model.Data = Encoding.UTF8.GetBytes((string)data+"$");
                        break;
                    case EncodingOption.BASE64:
                        model.Data = Convert.FromBase64String((string)data + "$");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Cannot pack package => wrong data type");
            }

            UpdatedQueue.Add(model);
            model.UpdatedIndex = UpdatedQueue.IndexOf(model);
        }
    }
}