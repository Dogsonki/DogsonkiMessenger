using Client.Models;
using Client.Networking.Core;
using Client.Networking.Model;
using System.Collections.ObjectModel;

namespace Client.Pages;

public partial class MainPage : ContentPage
{
	public static ObservableCollection<PersonFoundModel> LastChats { get; set; } = new ObservableCollection<PersonFoundModel>();
	public static MainPage Instance;

	public static void AddLastUsers(SearchModel[] users)
    {
		foreach(SearchModel user in users) 
		{
			LastChats.Add(new PersonFoundModel(UserModel.CreateOrGet(user.Username, user.ID)));
		}
    }

    protected override bool OnBackButtonPressed()
    {
		LocalUser.Logout();
        return base.OnBackButtonPressed();
    }

    public MainPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
		AddLastUsers(new SearchModel[] { new SearchModel("uwu", 11) });
        if (Instance is null)
			Instance = this;
	}

	private async void SetingsTapped(object sender, EventArgs e) => await Navigation.PushAsync(new ProfileSettingsPage());

    private void SearchPressed(object sender, EventArgs e)
    {
		SocketCore.Send(((SearchBar)sender).Text, Token.SEARCH_USER);
		Navigation.PushAsync(new SearchPage(((SearchBar)sender).Text));
    }
}