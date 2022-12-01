using Client.Models.Bindable;
using Client.Networking.Core;
using System.Collections.ObjectModel;
using Client.Models;
using Client.Networking.Packets;
using Client.Networking.Packets.Models;
using Newtonsoft.Json;
using Client.Networking.Models;

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

        SocketCore.SendCallback(new SearchPacket(preInputText, true), Token.SEARCH_USER, ParseFound);
    }

    private void SearchPressed(object sender, EventArgs e)
    {
        UsersFound.Clear();

        string input = ((Entry)sender).Text;

        if (!string.IsNullOrEmpty(input) && input.Length > 3)
        {
            SocketCore.SendCallback(new SearchPacket(input,true), Token.SEARCH_USER, ParseFound);
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
                    UsersFound.Add(new AnyListBindable(_,new Command(() => Conversation.OpenChat(_))));
                }
                else
                {
                    Group _ = Group.CreateOrGet(user.Username, user.Id);
                    UsersFound.Add(new AnyListBindable(_,new Command(() => Conversation.OpenChat(_))));
                }
            });
        }
    }
}