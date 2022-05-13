using System;
using System.Collections.Generic;
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
        public string ContentSend;
        public string ContentRecived;
        public object ContentExpected;
        protected int CallbackID;

        public RequestedCallback(Action<string> callback, string contentsend, object expected)
        {
            Callback = callback;
            ContentExpected = expected;
            ContentRecived = null;
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

        public static void InvokeByToken(int token, string rev)
        {
            for (int i = 0; i < Callbacks.Count; i++)
            {
                if (Callbacks[i] != null)
                {
                    if (Callbacks[i].GetToken() == token)
                    {
                        Callbacks[i].Invoke(rev);
                        return;
                    }
                }
            }
            Console.WriteLine("Cannot find RequestedCallback with token: " + token + " With recived: " + rev);
        }

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
