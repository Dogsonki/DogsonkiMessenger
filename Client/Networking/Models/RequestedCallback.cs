namespace Client.Networking.Model
{
    //Add lifetime to callbacks, can be used as memory leak where cannot be invoked 
    public class RequestedCallback
    {
        public static List<RequestedCallback> Callbacks { get; set; } = new List<RequestedCallback>(5000);
        protected Action<object> Callback;

        public object ContentSend;
        private int CallbackID;

        public RequestedCallback(Action<object> callback, object data, int pretoken)
        {
            CallbackID = pretoken;
            Callback = callback;
        }

        public static void AddCallback(RequestedCallback callback) => Callbacks.Add(callback);

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
                    MainThread.BeginInvokeOnMainThread(() => Callback.Invoke(Recived));
                }
                catch (Exception ex)
                {
                    Debug.Write(ex);
                }
            }
            Callbacks.Remove(this);
        }
    }
}