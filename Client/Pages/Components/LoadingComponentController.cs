namespace Client.Pages.Components;

public class LoadingComponentController
{
    private bool _isLoading = true;

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            ChangeLoadingState?.Invoke(value);
        }
    }

    public Action<bool>? ChangeLoadingState { get; set; } = null;
}