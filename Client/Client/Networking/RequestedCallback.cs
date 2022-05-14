using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Client.Networking
{
    public struct SocketMessageModel
    {
        public string Data;
        public int UpdatedIndex;
        public int InSendIndex;
    }

    public class RequestedCallback
    {
        public static List<RequestedCallback> Callbacks { get; set; } = new List<RequestedCallback>(5000);
         
        protected Action<string> Callback;
        protected Task<string> AsyncCallback;

        public string ContentSend;
        public string ContentRecived;
        protected int CallbackID;
        private bool isAsync = false; 

        public RequestedCallback(Action<string> callback, string contentsend, int pretoken)
        {
            CallbackID = pretoken;
            Callback = callback;
            ContentRecived = null;
            ContentSend = GetToken() + contentsend;
        }

        public RequestedCallback(Task<string> callback, string contentsend, int pretoken)
        {
            AsyncCallback = callback;
            ContentRecived = null;
            ContentSend = GetToken() + contentsend;
            CallbackID = pretoken;
        }

        public int GetToken() => CallbackID;

        public bool Invoke(string Recived)
        {
            bool _r = false;
            if (Callback != null)
            {
                try
                {
                    Console.WriteLine("Calling Action += "+Callback.Method.Name);
                    Device.BeginInvokeOnMainThread(() => Callback.Invoke(Recived));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                _r = true;
            }
            // Callbacks[Callbacks.FindIndex(x => x == this)] = null;
            //Callbacks.Remove(this); //Hope it won't crash app 
            return _r;
        }
    }
}
