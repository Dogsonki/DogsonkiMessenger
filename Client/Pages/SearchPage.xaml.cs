using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Client.Networking.Packets;

namespace Client.Pages;

public partial class SearchPage : ContentPage
{
    private static SearchPage Current;
    public static ObservableCollection<AnyListBindable> UsersFound { get; set; } = new ObservableCollection<AnyListBindable>();

    public SearchPage(string preInputText)
    {
        Current = this;
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

        if (users is null || users?.Count == 0)
        {
            Current.NoResultsMessage.IsVisible = true;
            return;
        }

        Current.NoResultsMessage.IsVisible = false;

        foreach (var user in users)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if(!user.isGroup)
                {
                    User _ = User.CreateOrGet(user.Username, user.Id);
                    UsersFound.Add(new AnyListBindable(_,new Command(() => User.OpenChat(_))));
                }
                else
                {
                    Group _ = Group.CreateOrGet(user.Username, user.Id);
                    UsersFound.Add(new AnyListBindable(_,new Command(() => Group.OpenChat(_))));
                }
            });
        }
    }
}