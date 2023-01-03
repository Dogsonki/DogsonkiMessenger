using Microsoft.AspNetCore.Components;
using Client.Utility;

namespace Client.Models.Navigation;

public class GlobalNavigation : ComponentBase
{
    [Inject]
    public NavigationManager _navigation { get; set; }

    public GlobalNavigation(NavigationManager navigation)
    {
        _navigation = navigation;
    }

    public async void NavigateTo(string pageName)
    {
        if (_navigation is null)
        {
            Debug.Error("Global navigation was null");
            return;
        }
        await Task.Run(() => { _navigation.NavigateTo(pageName); });
    }
}