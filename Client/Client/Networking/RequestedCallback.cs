﻿using System;
using System.Collections.Generic;
using Client.Utility;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Client.Networking
{

    public class RequestedCallback
    {
        //Callbacks might break sametimes when recive packet in wrong time
        public static List<RequestedCallback> Callbacks { get; set; } = new List<RequestedCallback>(5000);
         
        protected Action<string> Callback;
        protected Task<string> AsyncCallback;

        public string ContentSend;
        public string ContentRecived;
        protected int CallbackID;

        public RequestedCallback(Action<string> callback, string contentsend, int pretoken)
        {
            CallbackID = pretoken;
            Callback = callback;
            ContentRecived = null;
            ContentSend = GetToken() + contentsend;
        }

        public int GetToken() => CallbackID;
        public static int GetCount() => Callbacks.Count;

        /// <summary>
        /// Invokes function and removes itself from list of callbacks
        /// </summary>
        /// <param name="Recived"></param>
        /// <returns></returns>
        public bool Invoke(string Recived)
        {
            bool _r = false;
            if (Callback != null)
            {
                try
                {
                    Debug.Write("Invoking Action += "+Callback.Method.Name);
                    Device.BeginInvokeOnMainThread(() => Callback.Invoke(Recived));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                _r = true;
            }
            Callbacks.Remove(this);
            return _r;
        }
    }
}
