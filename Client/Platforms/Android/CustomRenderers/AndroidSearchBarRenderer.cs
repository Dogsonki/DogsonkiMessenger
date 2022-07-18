using Android.Content;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

[assembly: ExportRenderer(typeof(SearchBar), typeof(Client.Android.AndroidSearchBarRenderer))]
namespace Client.Android;

public class AndroidSearchBarRenderer : Microsoft.Maui.Controls.Handlers.Compatibility.VisualElementRenderer<SearchBar>
{
    public AndroidSearchBarRenderer(Context context) : base(context)
    {
        
    }
}
