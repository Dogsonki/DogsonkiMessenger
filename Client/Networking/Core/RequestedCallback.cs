using Client.Networking.Models;
using Client.Utility;

namespace Client.Networking.Core
{
    //Add lifetime to callbacks, can be used as memory leak where cannot be invoked 
    public class RequestedCallback
    {
        public static List<RequestedCallbackModel> Callbacks { get; set; } = new List<RequestedCallbackModel>(5000);

        public static bool IsAlreadyQueued(Token token) 
        {
            RequestedCallbackModel? callback = Callbacks.Find(x => x?.GetToken() == (int)token);

            return callback is not null;
        }

        public static void AddCallback(RequestedCallbackModel callback) => Callbacks.Add(callback);

        public static int GetCount() => Callbacks.Count;

        private static void RemoveCallback(int token)
        {
            RequestedCallbackModel? model = Callbacks.Find(x => x.GetToken() == token);

            if(model is null)
            {
                return;
            }

            if(Callbacks.Count > 0)
            {
                try
                {
                    Callbacks.Remove(model);
                }
                catch(Exception e) 
                {
                    Logger.PushException(e);
                }
            }
        }

        public static bool InvokeCallback(int token, SocketPacket data)
        {
            foreach (var callback in Callbacks)
            {
                if (callback.GetToken() == token)
                {
                    RemoveCallback(token);
                    callback.Invoke(data);
                    return true;
                }
            }
            return false;
        }
    }
}