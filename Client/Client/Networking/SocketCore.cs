using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Client.Utility;
using Newtonsoft.Json;
using Xamarin.Forms;

//For now only works with booleans and numbers
public class RequestedCallback 
{
    public static List<RequestedCallback> Callbacks { get; set; } = new List<RequestedCallback>(5000);
    
    protected Action<string> Callback;
    public string ContentSend;
    public string ContentRecived;
    public object ContentExpected;
    protected int CallbackID;

    public RequestedCallback(Action<string> callback, string contentsend,object expected)
    {
        Callback = callback;
        ContentExpected = expected;
        ContentRecived = null;
        CallbackID = GetNewCallbackID();
        ContentSend = GetToken() + contentsend;
    }

    public RequestedCallback(Action<string> callback, string contentsend, object expected, int PreToken)
    {
        Callback = callback;
        ContentExpected = expected;
        ContentRecived = null;
        CallbackID = PreToken;
        ContentSend = GetToken() + contentsend;
    }

    public int GetToken() => CallbackID;

    public static void InvokeByToken(int token,string rev)
    {
        for (int i = 0; i < Callbacks.Count; i++)
        {
            if(Callbacks[i] != null)
            {
                if(Callbacks[i].GetToken() == token)
                {
                    Callbacks[i].Invoke(rev);
                    return;
                }
            }
        }
        Console.WriteLine("Cannot find RequestedCallback with token: " + token + " With recived: " + rev);
    }

    int GetNewCallbackID()
    {
        for (int i = 0; i < Callbacks.Count; i++)
        {
            if (Callbacks[i] == null)
            {
                return i;
            }
        }
        return Callbacks.Count;
#if usebettersystem
        Random rnd = new Random();

        bool _new = false;
        int _gen = rnd.Next(1, 9999);
        int _itr = 0;

        while (!_new)
        {
            foreach(var _ in Callbacks)
            {
                if(_.CallbackID == _gen)
                {
                    _new = false;
                }
            }
            _itr++;
            if(_itr == 9999)
            {
                throw new Exception($"Cannot generate new Callback id {Callbacks.Count} {ContentSend}");
            }
        }
        return _gen;
#endif
    }

    public bool Invoke(string Recived)
    {
        bool _r = false;
        if(Callback != null)
        {
            try
            {
                Callback.Invoke(Recived);
            }
           catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            _r = true;
        }
        Callbacks[Callbacks.FindIndex(x => x == this)] = null;
        //Callbacks.Remove(this); //Hope it won't crash app 
        return _r;
    }
}

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
        protected static bool IsConnected { get; set; }
        private const int MaxBuffer = 254;

        public static void Init() //Todo set stopwatch 
        {
            Config = SocketConfig.ReadConfig();

            Stopwatch ConnectTime = new Stopwatch();
            ConnectTime.Start();
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
            ReciveThread.Start();

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
                    Console.WriteLine("Recived: "+rev);
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
                                    Console.WriteLine("Callback found: " + tk);
                                    req.Invoke(rev.Substring(5));//dont send token as parameter
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
                } 
                Thread.Sleep(10);
            }
        }

        protected string Destination;
        private static void ReadRawBuffer(string RecivedMessage)
        {
            Console.WriteLine("Reading raw buffer: " + RecivedMessage);
            if(RecivedMessage == MainUser.Username)
            {
                
            }
        }

        private static bool GetToken(string req, out int tk)
        {
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

        public static void SendR(Action<string> callback, string data, object expected, int Token, bool isAync = true)
        {
            RequestedCallback.Callbacks.Add(new RequestedCallback(callback, data, expected,Token));
            SendRaw(data);//Write idcallback too !
        }

        public static void SendRaw(string data, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
        {
            try
            {
                if (data is null)
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

                Byte[] _buf = Encoding.UTF8.GetBytes(data);
                try
                {
                    Stream.Write(_buf, 0, _buf.Length);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Unregistred exception: " + ex);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
