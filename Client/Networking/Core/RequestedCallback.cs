﻿using Client.Networking.Models;

namespace Client.Networking.Core
{
    //Add lifetime to callbacks, can be used as memory leak where cannot be invoked 
    public class RequestedCallback
    {
        public static List<RequestedCallbackModel> Callbacks { get; set; } = new List<RequestedCallbackModel>(5000);

        public static bool IsAlreadyQueued(Token token) => Callbacks.Find(x => x.GetToken() == (int)token) is not null;

        public static void AddCallback(RequestedCallbackModel callback) => Callbacks.Add(callback);

        public static int GetCount() => Callbacks.Count;

        private static void RemoveCallback(int token)
        {
            RequestedCallbackModel? model = Callbacks.Find(x => x.GetToken() == token);
            if(model is null)
            {
                return;
            }
            Callbacks.Remove(model);
        }

        public static bool InvokeCallback(int token, object data)
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