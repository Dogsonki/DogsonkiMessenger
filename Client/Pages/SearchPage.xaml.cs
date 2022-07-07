using Client.Models;
using Client.Networking.Core;
using Client.Networking.Model;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace Client.Pages;

public partial class SearchPage : ContentPage
{
    public static ObservableCollection<User> UsersFound { get; set; } = new ObservableCollection<User>();

    public SearchPage(string preInputText)
	{
		InitializeComponent();
        SearchInput.Text = preInputText;
        NavigationPage.SetHasNavigationBar(this, false);
	}

    private void SearchPressed(object sender, EventArgs e)
    {
        string input = ((SearchBar)sender).Text;
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
            UsersFound.Add(User.CreateOrGet(user.Username, user.ID));
        }
    }
}