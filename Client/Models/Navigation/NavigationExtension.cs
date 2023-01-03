using Microsoft.AspNetCore.Components.Routing;

namespace Client.Models.Navigation;

public static class NavigationExtension
{
    /// <summary>
    /// Returns page name from location changed event ex. /MainPage
    /// </summary>
    /// <param name="navigation"></param>
    public static string GetPageName(this LocationChangedEventArgs navigation)
    {
        return navigation.Location.Substring(15);
    }
}