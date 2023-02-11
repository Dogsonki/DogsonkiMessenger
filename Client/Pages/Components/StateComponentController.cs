namespace Client.Pages.Components;

public class StateComponentController
{
    private bool _state = true;

    public bool State
    {
        get => _state;
        set
        {
            _state = value;
            ChangeState?.Invoke(value);
        }
    }

    public Action<bool>? ChangeState { get; set; } = null;
}


public class StateComponentController<T>
{
    private T? _state = default;

    public T? State
    {
        get => _state;
        set
        {
            _state = value;
            ChangeState?.Invoke(value);
        }
    }

    public Action<T?>? ChangeState { get; set; } = null;
}