using Newtonsoft.Json;

namespace Client.Networking.Model
{
    //Add lifetime to callbacks, can be used as memory leak where cannot be invoked 
    public class RequestedCallback<T>
    {
        public static List<RequestedCallbackModel<T>> Callbacks { get; set; } = new List<RequestedCallbackModel<T>>(5000);

        public static bool IsAlreadyQueued(Token token) => Callbacks.Find(x => x.GetToken() == (int)token) is not null;
        public static void AddCallback(RequestedCallbackModel<T> callback)
        {
            Debug.Write("added");
            Callbacks.Add(callback);
        }


        public static int GetCount() => Callbacks.Count;

        public static bool InvokeCallback(int token, string data)
        {
            T? _;
            foreach(var callback in Callbacks)
            {
                if(callback.GetToken() == token)
                {
                    if(callback.ParameterType == typeof(string) || callback.ParameterType == typeof(int))
                    {
                        callback.Invoke((T)Convert.ChangeType(data,typeof(T)));
                        return true;
                    }
                    else
                    {
                        _ = JsonConvert.DeserializeObject<T>(data);

                        if(_ is not null)
                        {
                            callback.Invoke(_);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}