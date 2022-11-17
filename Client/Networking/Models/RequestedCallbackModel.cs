﻿using Client.Utility;
using System.Reflection;

namespace Client.Networking.Model;

public class RequestedCallbackModel
{
    private Action<object> CallbackAction;

    private int CallbackToken { get; init; }

    public RequestedCallbackModel(Action<object> callback, int pretoken)
    {
        CallbackToken = pretoken;
        CallbackAction = callback;
    }

    public int GetToken() => CallbackToken;

    /// <summary>
    /// Invokes function and removes itself from list of callbacks
    /// </summary>
    public void Invoke(object Recived)
    {
        if (CallbackAction != null)
        {
            MainThread.BeginInvokeOnMainThread(() => 
            {
                try
                {
                    CallbackAction.Invoke(Recived);
                }
                catch(TargetInvocationException exception)
                {
                    Logger.Push(exception, TraceType.Func, LogLevel.Error);
                }
            });
        }
    }
}