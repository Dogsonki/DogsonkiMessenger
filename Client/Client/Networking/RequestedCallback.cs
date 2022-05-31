using Client.Utility;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Client.Networking
{
    public class RequestedCallback
    {
        //Callbacks might break sametimes when recive packet in wrong time
        public static List<RequestedCallback> Callbacks { get; set; } = new List<RequestedCallback>(5000);

        protected Action<object> Callback;

        public object ContentSend;
        protected int CallbackID;

        public RequestedCallback(Action<object> callback, object data, int pretoken)
        {
            CallbackID = pretoken;
            Callback = callback;
        }

        public int GetToken() => CallbackID;
        public static int GetCount() => Callbacks.Count;

        /// <summary>
        /// Invokes function and removes itself from list of callbacks
        /// </summary>
        /// <param name="Recived"></param>
        /// <returns></returns>
        public void Invoke(object Recived)
        {
            if (Callback != null)
            {
                try
                {
                    Debug.Write("Invoking Action += " + Callback.Method.Name);
                    Device.BeginInvokeOnMainThread(() => Callback.Invoke(Recived));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            Callbacks.Remove(this);
        }
    }
}
