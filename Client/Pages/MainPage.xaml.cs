using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

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
        view.ScrollToAsync(0d, 0d, true);
        return true;
    }

    public MainPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        if (Instance is null)
        {
            Instance = this;

            Logger.Push("Main page initialized", TraceType.Func, LogLevel.Debug);
        }

        for(int i = 0;i < 25; i++)
        {
            LastChats.Add(new AnyListBindable(User.CreateOrGet($"Test User{i}", (uint)i)));
        }
	}

	private async void SettingsTapped(object sender, EventArgs e) => await Navigation.PushAsync(new SettingsPage());

    private async void SearchPressed(object sender, EventArgs e)
    {
        try
        {
            string SearchInput = ((Entry)sender).Text;
            if (string.IsNullOrEmpty(SearchInput))
            {
                return;
            }
            SocketCore.Send(SearchInput, Token.SEARCH_USER);
            await Navigation.PushAsync(new SearchPage(SearchInput));
        }
        catch(Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }
}