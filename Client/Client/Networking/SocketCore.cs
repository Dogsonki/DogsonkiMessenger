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
using System.Threading.Tasks;
using Client.Networking.Config;
using System.IO;
using Xamarin.Forms;
using System.Reflection;

namespace Client.Networking
{
    public enum EncodingOption
    {
        UFT8,
        BASE64,
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

        private const int MaxBuffer = 1024 * 8;

        public static void Init()
        {
            Config = SocketConfig.ReadConfig();
#region Connect
            try
            {
                Client = new TcpClient(Config.Ip, Config.Port);
                Stream = Client.GetStream();
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Couldn't connect: " + ex);
                return;
            }

            Client.ReceiveBufferSize = MaxBuffer;
            Client.SendBufferSize = MaxBuffer;

            IsConnected = true;
            ReciveThread = new Thread(Recive);
            ManageSendingQueue = new Thread(MenageQueue);

            ReciveThread.Start();
            ManageSendingQueue.Start();
#endregion
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
                            
                            case EncodingOption.UFT8:
                                DecodedString = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                                break;

                            case EncodingOption.BASE64:
                                 DecodedString = Convert.ToBase64String(buffer);
                                break;
                        }

                        for (int i = 0; i < RequestedCallback.Callbacks.Count; i++)
                            try
                            {
                                int tk;
                                RequestedCallback req = RequestedCallback.Callbacks[i];
                                if (GetToken(DecodedString, out tk))
                                {
                                    if (tk == req.GetToken())
                                    {
                                        //Bug: if we take length of rev it will return 254 for same reason 
                                        //this could be by convering byte
                                        req.Invoke(DecodedString.Substring(5));
                                        _WillReadRaw = false;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Cannot get token from recived message:" + DecodedString);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }

                        if (_WillReadRaw)
                            ReadRawBuffer(Encoding.UTF8.GetString(buffer));

                        //idk if i should do that but it works 
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

        protected static List<string> m_ReadingImage = new List<string>();
        private static void ReadRawBuffer(string RecivedMessage)
        {
            int token;
            if (!GetToken(RecivedMessage, out token))//TODO: Still can write sametimes to reading image
            {
                if (m_ReadNextImage)
                {
                    m_ReadingImage.Add(RecivedMessage);
                }
                return;
            }

            switch (token)
            {
                case 0003:
                    Device.BeginInvokeOnMainThread(() => Application.Current.MainPage.Navigation.PushAsync(new MessageView()));
                    break;
                case 0004:
                    PeopleFinder.ParseQuery(RemoveToken(RecivedMessage));
                    break;
                case 0005:
                    try
                    {
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
                        m_ReadNextImage = false;
                        StringBuilder br = new StringBuilder();
                        
                        foreach(var b in m_ReadingImage)
                        {
                            br.Append(b);
                        }

                        SendRaw(br.ToString());
                        Device.BeginInvokeOnMainThread(() => MainAfterLoginPage.h.Source = ImageSource.FromStream(
                            () => new MemoryStream(Encoding.UTF8.GetBytes(br.ToString()))));
                    }   
                    else
                    {
                        ActualDecoding = EncodingOption.BASE64;
                        m_ReadNextImage = true;
                    }
                    break;
                default:
                    Console.WriteLine($"Cannot find raw definition {RecivedMessage}");
                    break;
            }
        }

        private static string RemoveToken(string req) => req.Substring(5);

        public static void SendImage(Type type, string resourceName)
        {
            byte[] bytes;

            using (Stream sr = IntrospectionExtensions.GetTypeInfo(type.GetType()).Assembly.GetManifestResourceStream($"Client.Pages.{resourceName}"))
            {
                if (sr == null)
                {
                    Console.WriteLine("Stream of image is null");
                    return;
                }
                
                bytes = new byte[sr.Length];
                sr.Read(bytes, 0, bytes.Length);
            }

            if (bytes.Length < 0)
            {
                Console.WriteLine("Stream have 0 bytes"); //idk if it's possible just to be sure to not send 0 bytes
                return;
            }
              

            SendRaw("3");
            SendRaw(bytes);
            SendRaw("ENDFILE");

            bytes = null;
        }

        private static bool GetToken(string req, out int tk)
        {
            if (req.Length == 0)
            {
                tk = -1;
                return false;
            }
            //Token has max 4 len and -
            //ex. 0003-
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

        /// <summary>
        /// TODO: Make async happen
        /// </summary>
        public static void SendR(Task<string> callback, string data, int Token)
        {
            RequestedCallback.Callbacks.Add(new RequestedCallback(callback,data,Token));
            SendRaw(data);//Write IDcallback too !
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
                            Byte[] _buf;
                            try
                            {
                                _buf = model.Data; 

                                Stream.Write(_buf, 0, _buf.Length);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Unregistred exception: " + ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        Thread.Sleep(100);
                        SendingQueue.Remove(model);
                    }
                }
                if(UpdatedQueue.Count > 0)
                {
                    //Check if there is no memory leak from infinite adding Packages to UpdatedQueue
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

        //For now it works only for JPG, and default raw variables 

        /// <summary>
        /// Packs package and preparing how to send it 
        /// </summary>
        /// <param name="data"></param>
        private static void AddToQueue(object data, EncodingOption option = EncodingOption.UFT8)
        {
            SocketMessageModel model = null;
            if (data.GetType() == typeof(byte[]))
            {
                if (((byte[])data).Length == 0)
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
                        model.Data = Encoding.UTF8.GetBytes((string)data);
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