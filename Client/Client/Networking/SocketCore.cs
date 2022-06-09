using Client.IO;
using Client.Model.Session;
using Client.Models;
using Client.Networking.Config;
using Client.Networking.Model;
using Client.Pages;
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
    public static class SocketCore
    {
        private static TcpClient Client { get; set; }
        private static NetworkStream Stream { get; set; }
        private static SocketConfig Config { get; set; }
        private static Thread ReciveThread { get; set; }
        private static Thread ManageSendingQueue { get; set; }
        private static bool IsConnected { get; set; }
        private static bool Initialized = false;
        private static bool PendingConnection = false;
        private const int MaxBuffer = 1024*18;

        public static bool Init()
        {
            try
            {
                Config = SocketConfig.ReadConfig();
                Client = new TcpClient(Config.Ip, Config.Port);
            }
            catch(Exception ex)
            {
                Debug.Error(ex);
                return false;
            } 
            if (Client != null)
            {
                Stream = Client.GetStream();
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

        public static bool TryConnect()
        {
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

        private static string LongBuffer = "";

        private static void ProcessBuffer(string buffer)
        {
            SocketPacket packet = null;

            try
            {
                packet = JsonConvert.DeserializeObject<SocketPacket>(buffer);  
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                return;
            }
            for (int i = 0; i < RequestedCallback.GetCount(); i++)
            {
                RequestedCallback req = RequestedCallback.Callbacks[i];
                if (packet.Token == req.GetToken())
                {
                    req.Invoke(packet.Data);
                    return;
                }
            }
            ReadRawBuffer(packet);
        }

        private static void Recive()
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
                            string DecodedString = Encoding.UTF8.GetString(buffer, 0, LenBuffer);
                            if (string.IsNullOrEmpty(DecodedString))
                                continue;

                            LongBuffer += DecodedString;

                            string buff;
                            int indexDollar = 1;
                            while(indexDollar > 0)
                            {
                                indexDollar = LongBuffer.IndexOf('$');
                                if (indexDollar == -1)
                                    break;
                                buff = LongBuffer.Substring(0, indexDollar);
                                LongBuffer = LongBuffer.Substring(indexDollar+1);
                                ProcessBuffer(buff);
                            }
                            Stream.Flush();
                            buffer = new byte[MaxBuffer];
                        }
                    }
                    catch(Exception ex)
                    {
                        if(ex is InvalidCastException)
                        {
                            Debug.Error("Error in casting buffer into packet " + ex);
                        }
                        else
                        {
                            RedirectConnectionLost(ex);
                        }
                    }
                }

                Thread.Sleep(10);
            }
            catch (Exception ex) //TODO: rework this, sametimes can't see [CallerLineNumber] hard to debugging
            {
                RedirectConnectionLost(ex);

                //BUG: sametimes it doens't work for same reason 
            }
        }

        private static void RedirectConnectionLost(Exception ex,  [CallerLineNumber] int lineNumber = 0)
        {
            Debug.Error(lineNumber+"::"+ex);
            Device.BeginInvokeOnMainThread(() =>
            {
                StaticNavigator.PopAndPush(new AppEntry(false));
            });
        }

        private static void MenageQueue()
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
                Thread.Sleep(10);
            }
        }

        private static void ReadRawBuffer(SocketPacket packet)
        {
            switch ((Token)packet.Token)
            {
                case Token.ERROR:
                    Debug.Error("TOKEN -2");
                    break;
                case Token.SEARCH_USER:
                    SearchPageView.ParseQuery(packet.Data);
                    break;
                case Token.CHAT_MESSAGE:
                    try
                    {
                        Device.InvokeOnMainThreadAsync(() => MessagePageView.AddMessage(((JObject)packet.Data).ToObject<MessageModel>()));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cannot parse MessageModel {ex}");
                    }
                    break;
                case Token.SESSION_INFO:
                    Session session = ((JObject)packet.Data).ToObject<Session>();
                    StorageIO.Write(session, "session");
                    break;
                case Token.LOGIN_SESSION:
                    LoginCallbackModel model = ((JObject)packet.Data).ToObject<LoginCallbackModel>();
                    if(model.Token == 1)
                    {
                        LocalUser.username = model.Username;
                        LocalUser.isLoggedIn = true;
                        LocalUser.id = model.ID.ToString();
                        Device.BeginInvokeOnMainThread(() => StaticNavigator.PopAndPush(new MainAfterLoginPage()));
                    }
                    break;
                case Token.AVATAR_REQUEST:
                    UserImageRequest img = ((JObject)packet.Data).ToObject<UserImageRequest>();

                    string avat = img.ImageData.Substring(2);
                    avat = avat.Substring(0, avat.Length - 1);

                    var user = UserModel.GetUser(img.UserID);
                    byte[] imgBuffer = Convert.FromBase64String(avat);

                    Device.BeginInvokeOnMainThread(() => user.Avatar = ImageSource.FromStream(() => new MemoryStream(imgBuffer)));
                  
                    break;
                case Token.LAST_USERS:
                    SearchModel[] users = ((JArray)packet.Data).ToObject<SearchModel[]>();
                    foreach (SearchModel x in users)
                    {
                        MainAfterLoginPageView.AddLastUser(x);
                    }
                    break;
                default:
                    Console.WriteLine($"Cannot find raw definition");
                    break;
            }
        }

        public static bool SendR(Action<object> Callback, object SendingData, Token token)
        {
            if (!AbleToSend())
                return false;

            RequestedCallback.Callbacks.Add(new RequestedCallback(Callback, SendingData, (int)token));
            Send(SendingData, token);
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

        public static bool Send(object data, Token token = Token.EMPTY, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
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