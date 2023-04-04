using Microsoft.JSInterop;

namespace Client.Models.JavaScriptServices;

internal class BackButtonService
{
    private IJSRuntime JsRuntime;

    public BackButtonService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async void GoBack()
    {
        await JsRuntime.InvokeVoidAsync("NavigationGoBack");
    }
}