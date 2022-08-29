using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace Client.Pages;

public partial class SearchPage : ContentPage
{
    public static ObservableCollection<AnyListBindable> UsersFound { get; set; } = new ObservableCollection<AnyListBindable>();

    public SearchPage(string preInputText)
    {
        UsersFound.Clear();

        InitializeComponent();
        SearchInput.Text = preInputText;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private void SearchPressed(object sender, EventArgs e)
    {
        UsersFound.Clear();

        string input = ((Entry)sender).Text;

        if (!string.IsNullOrEmpty(input))
        {
            SocketCore.Send(input, Token.SEARCH_USER);
        }
    }

    public static void ParseFound(object req)
    {
        List<SearchModel> users = ((JArray)req).ToObject<List<SearchModel>>();

        foreach (var user in users)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if(!user.isGroup)
                {
                    UsersFound.Add(new AnyListBindable(User.CreateOrGet(user.Username, user.Id), true));
                }
                else
                {
                    UsersFound.Add(new AnyListBindable(Group.CreateOrGet(user.Username, user.Id),true));
                }
            });
        }
    }
}