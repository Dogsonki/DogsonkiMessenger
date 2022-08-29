namespace Client.Networking.Model;

public class RequestedCallbackModel<T>
{
    protected Action<T> CallbackAction;

    protected int CallbackToken { get; init; }

    public Type ParameterType { get; init; }

    public RequestedCallbackModel(Action<T> callback, int pretoken)
    {
        CallbackToken = pretoken;
        CallbackAction = callback;
        ParameterType = typeof(T);
    }

    public int GetToken() => CallbackToken;

    /// <summary>
    /// Invokes function and removes itself from list of callbacks
    /// </summary>
    public void Invoke(T Recived)
    {
        if (CallbackAction != null)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() => CallbackAction.Invoke(Recived));
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }
    }
}