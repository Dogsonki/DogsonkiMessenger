using Client.Networking.Model;

namespace Client.Networking.Core
{
    //Add lifetime to callbacks, can be used as memory leak where cannot be invoked 
    public class RequestedCallback
    {
        public static List<RequestedCallbackModel> Callbacks { get; set; } = new List<RequestedCallbackModel>(5000);

        public static bool IsAlreadyQueued(Token token) => Callbacks.Find(x => x.GetToken() == (int)token) is not null;

        public static void AddCallback(RequestedCallbackModel callback) => Callbacks.Add(callback);

        public static int GetCount() => Callbacks.Count;

        public static bool InvokeCallback(int token, string data)
        {
            foreach (var callback in Callbacks)
            {
                if (callback.GetToken() == token)
                {
                    Callbacks.Remove(Callbacks.Find(x => x.GetToken() == token));
                    callback.Invoke(data);
                    return false;
                }
            }
            return true;
        }

    }
}