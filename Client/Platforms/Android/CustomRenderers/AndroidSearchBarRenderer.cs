using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Android.Content;

[assembly: ExportRenderer(typeof(SearchBar), typeof(Client.Android.AndroidSearchBarRenderer))]
namespace Client.Android;

public class AndroidSearchBarRenderer : SearchBarRenderer
{
    public AndroidSearchBarRenderer(Context context) : base(context)
    {

    }

    protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
    {
        base.OnElementChanged(e);

        if (Control != null) { }
    }
}
