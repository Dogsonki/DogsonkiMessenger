using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Client.Utility;
using Newtonsoft.Json;
using Xamarin.Forms;
using Client.Pages;
using Client.Views;
using Client.Models;
using System.Threading.Tasks;
using Client.Networking.Config;

namespace Client.Networking
{
    public class SocketCore
    {
        protected static TcpClient Client { get; set; }
        protected static NetworkStream Stream { get; set; }
        protected static SocketConfig Config { get; set; }
        protected static Thread ReciveThread { get; set; }
        protected static Thread ManageSendingQueue { get; set; }
        protected static bool IsConnected { get; set; }
        private const int MaxBuffer = 1024;

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

            IsConnected = true;
            ReciveThread = new Thread(Recive);
            ManageSendingQueue = new Thread(MenageQueue);

            ReciveThread.Start();
            ManageSendingQueue.Start();
#endregion
        }

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
                        string rev = Encoding.UTF8.GetString(buffer);
                        Console.WriteLine($"Got buffer {rev}");
                        for (int i = 0; i < RequestedCallback.Callbacks.Count; i++)
                        {
                            try
                            {
                                int tk;
                                RequestedCallback req = RequestedCallback.Callbacks[i];
                                if (GetToken(rev, out tk))
                                {
                                    if (tk == req.GetToken())
                                    {
                                        //Bug: if we take length of rev it will return 254 for same reason 
                                        //this could be by convering byte
                                        req.Invoke(rev.Substring(5));
                                        _WillReadRaw = false;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Cannot get token from recived message:" + rev);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                        if (_WillReadRaw)
                            ReadRawBuffer(Encoding.UTF8.GetString(buffer));

                        //idk if i should do that but it works 
                        Stream.Flush();
                        buffer = new byte[MaxBuffer];
                        rev = String.Empty;
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
            if (!GetToken(RecivedMessage, out token))
                return;
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
                default:
                    Console.WriteLine("Cannot find raw definition");
                    break;
            }
        }

        private static string RemoveToken(string req) => req.Substring(5);

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
                            Byte[] _buf = Encoding.UTF8.GetBytes(model.Data);
                            try
                            {
                                Console.WriteLine($"Sending: [{model.Data}]");
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
                    SendingQueue = new List<SocketMessageModel>(UpdatedQueue);
                    UpdatedQueue.Clear();
                }

                Thread.Sleep(100);
            }
        }

        public static bool SendRaw(string data, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
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

        private static void AddToQueue(string data)
        {
            SocketMessageModel model = new SocketMessageModel() { Data = data };
            UpdatedQueue.Add(model);
            model.UpdatedIndex = UpdatedQueue.IndexOf(model);
        }
    }
}