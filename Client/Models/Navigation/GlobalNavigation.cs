using Client.Utility;
using Microsoft.AspNetCore.Components;

namespace Client.Models.Navigation;

public class GlobalNavigation : ComponentBase
{
    private static NavigationManager? Navigation { get; set; }

    public GlobalNavigation(NavigationManager navigation)
    {
        if (navigation is null)
        {
            Navigation = navigation;
        }
    }

    public static void NavigateTo(string pageName)
    {
        if (Navigation is null)
        {
            return;
        }

        Navigation.NavigateTo(pageName);
    }
}