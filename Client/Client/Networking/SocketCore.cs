using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Client.Utility;
using Newtonsoft.Json;
using Xamarin.Forms;
using Client.Pages;
using Client.Views;
using Client.Models;

namespace Client.Networking
{
    public class SocketConfig
    {
        [JsonProperty("Socket_IP")]
        public string Ip;
        [JsonProperty("Socket_PORT")]
        public int Port;

        public static SocketConfig ReadConfig()
        {
            string config = "";
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(SocketConfig)).Assembly;
                Stream stream = assembly.GetManifestResourceStream("Client.Networking.SocketConfig.json");
               
                using (var reader = new StreamReader(stream))
                {
                    config = reader.ReadToEnd();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (!string.IsNullOrEmpty(config))
            {
                return JsonConvert.DeserializeObject<SocketConfig>(config);
            }
            return null;
        }

        public static void SaveConfig(SocketConfig sc)
        {
            DependencyService.Get<IFileService>().CreateFile("SocketConfig.json", JsonConvert.SerializeObject(sc));
        }
    }


    public class SocketCore
    {
        protected static TcpClient Client { get; set; }
        protected static NetworkStream Stream { get; set; }
        protected static SocketConfig Config { get; set; }
        protected static Thread ReciveThread { get; set; }
        protected static Thread ManageSendingQueue { get; set; }
        protected static bool IsConnected { get; set; }
        private const int MaxBuffer = 1024;

        public static void Init() //Todo set stopwatch 
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

            while (IsConnected)
            {
                while((LenBuffer = Stream.Read(buffer,0,buffer.Length)) != 0)
                {
                    bool _WillReadRaw = true;
                    string rev = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine("Recived raw: " + rev);
                    for (int i = 0; i<RequestedCallback.Callbacks.Count;i++)
                    {
                        try
                        {
                            int tk;
                            RequestedCallback req = RequestedCallback.Callbacks[i];
                            if (GetToken(rev,out tk))
                            {
                                if(tk == req.GetToken())
                                {
                                    //Bug: if we take length of rev it will return 254 for same reason 
                                    //this could be by convering byte
                                    Console.WriteLine("Callback found: " + tk);
                                    Console.WriteLine("Calling callback with: " + rev.Substring(5));
                                    req.Invoke(rev.Substring(5));
                                    _WillReadRaw = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Cannot get token from recived message:" + rev);
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    if(_WillReadRaw)
                       ReadRawBuffer(Encoding.UTF8.GetString(buffer));

                    Stream.Flush();
                    buffer = new byte[MaxBuffer];
                    rev = String.Empty;
                }
       
                Thread.Sleep(10);
            }
        }

        private static void ReadRawBuffer(string RecivedMessage)
        {
            Console.WriteLine("Reading raw: " + RecivedMessage);
            int token;
            if (!GetToken(RecivedMessage, out token))
                return;
            switch (token)
            {
                case 0003:
                    Console.WriteLine("Navigating to MessageView");
                    Device.BeginInvokeOnMainThread(() => Application.Current.MainPage.Navigation.PushAsync(new MessageView()));
                    break;
                case 0004:
                    PeopleFinder.ParseQuery(RemoveToken(RecivedMessage));
                    break;
                case 0005:
                    MessageViewModel.AddMessage(JsonConvert.DeserializeObject<MessageModel>(RemoveToken(RecivedMessage)));  
                    break;
                default:
                    Console.WriteLine("Reading raw buffer: " + RecivedMessage);
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
            StringBuilder br = new StringBuilder();
            br.Append(req[0]);
            br.Append(req[1]);
            br.Append(req[2]);
            br.Append(req[3]);
            Console.WriteLine("Parsed Recived Token: " +br.ToString());
            if (int.TryParse(br.ToString(), out tk))
            {
                return true;
            }
            return false;
        }

        public static void SendR(Action<string> callback, string data,object expected,bool isAync = true)
        {
            RequestedCallback.Callbacks.Add(new RequestedCallback(callback, data, expected));
            SendRaw(data);
        }

        /// <summary>
        /// TODO: Make async happen
        /// </summary>
        public static void SendR(Action<string> callback, string data, object expected, int Token, bool isAync = true)
        {
            RequestedCallback.Callbacks.Add(new RequestedCallback(callback, data, expected,Token));
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
                                Console.WriteLine($"Sending .. {model.Data}");
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

        public static void SendRaw(string data, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
        {
            if (data is null || string.IsNullOrEmpty(data))
            {
                Console.WriteLine($"Data is null {path} line: {lineNumber}");
                return;
            }

            if (Client == null || Stream == null)
            {
                Console.WriteLine("Client is null");
                return; 
            }

            if (!IsConnected)
            {
                Console.WriteLine("Client is not connected to server");
                return;
            }

            if (!Client.Connected)
            {
                Console.WriteLine("Client disconnected form server");
                return;
            }
            Console.WriteLine("Adding to queue" + data);
            SocketMessageModel model = new SocketMessageModel() { Data= data };
            UpdatedQueue.Add(model);
            model.UpdatedIndex = UpdatedQueue.IndexOf(model);
        }
    }
}