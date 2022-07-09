using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;
using System.Collections.ObjectModel;

namespace Client.Pages;

public partial class MainPage : ContentPage
{
	public static ObservableCollection<AnyListBindable> LastChats { get; set; } = new ObservableCollection<AnyListBindable>();
	public static MainPage Instance;

	public static void AddLastUsers(SocketPacket packet)
    {
        SearchModel[] users = Essential.ModelCast<SearchModel[]>(packet.Data);
        List<AnyListBindable> bindable = new List<AnyListBindable>();

        Parallel.ForEach(users, (user) =>
        {
            bindable.Add(new AnyListBindable(User.CreateOrGet(user.Username, user.ID)));
        });

        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach(AnyListBindable use in bindable)
            {
                LastChats.Add(use);
            }
        });
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    public MainPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

#if DEBUG
  
#endif

        if (Instance is null)
        {
            Instance = this;
        }         
	}

	private async void SetingsTapped(object sender, EventArgs e) => await Navigation.PushAsync(new SettingsPage());

    private void SearchPressed(object sender, EventArgs e)
    {
		SocketCore.Send(((SearchBar)sender).Text, Token.SEARCH_USER);
		Navigation.PushAsync(new SearchPage(((SearchBar)sender).Text));
    }
}