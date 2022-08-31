namespace Client.Networking.Model;

public class RequestedCallbackModel
{
    protected Action<object> CallbackAction;

    protected int CallbackToken { get; init; }

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