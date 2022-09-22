using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using System.Collections.ObjectModel;
using Client.Networking.Packets;
using Newtonsoft.Json;

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

        SocketCore.SendCallback(ParseFound, preInputText, Token.SEARCH_USER);
    }

    private void SearchPressed(object sender, EventArgs e)
    {
        UsersFound.Clear();

        string input = ((Entry)sender).Text;

        if (!string.IsNullOrEmpty(input))
        {
            SocketCore.SendCallback(ParseFound, input, Token.SEARCH_USER);
        }
    }

    public void ParseFound(object req)
    {
        SearchModel[]? users = JsonConvert.DeserializeObject<SearchModel[]>((string)req);

        if (users is null || users?.Length == 0)
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