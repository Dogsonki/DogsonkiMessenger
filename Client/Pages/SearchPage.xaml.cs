using Client.Models;
using Client.Networking.Core;
using Client.Networking.Model;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace Client.Pages;

public partial class SearchPage : ContentPage
{
    public static ObservableCollection<PersonFoundModel> UsersFound { get; set; } = new ObservableCollection<PersonFoundModel>();

    public SearchPage(string preInputText)
	{
		InitializeComponent();
        SearchInput.Text = preInputText;
        NavigationPage.SetHasNavigationBar(this, false);
	}

    private void SearchPressed(object sender, EventArgs e)
    {
        SocketCore.Send(((SearchBar)sender).Text, Token.SEARCH_USER);
    }

    public static void ParseFound(object req)
    {
        List<SearchModel> users = ((JArray)req).ToObject<List<SearchModel>>();
        foreach (var user in users)
        {
            UserModel u = UserModel.CreateOrGet(user.Username, user.ID);
            UsersFound.Add(new PersonFoundModel(u));
        }
    }
}