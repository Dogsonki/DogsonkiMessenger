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
        List<AnyListBindable> bindable = new List<AnyListBindable>();
        Task.Run(() =>
        {
            SearchModel[] users = Essential.ModelCast<SearchModel[]>(packet.Data);

            foreach (var user in users)
            {
                Debug.Write(user.Username + " " + user.Username);
                var u = User.CreateOrGet(user.Username, user.Id);
                bindable.Add(new AnyListBindable(u, true));
            }
        }).ContinueWith((w) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (AnyListBindable use in bindable)
                {
                    LastChats.Add(use);
                }
            });
        });
    }

    public static void AddLastGroups(SocketPacket packet)
    {
        List<AnyListBindable> bindable = new List<AnyListBindable>();
        Task.Run(() =>
        {
            GroupCallbackModel[] groups = Essential.ModelCast<GroupCallbackModel[]>(packet.Data);

            foreach (var group in groups)
            {
                bindable.Add(new AnyListBindable(new Group(group.GroupName, group.GroupId)));
            }
        }).ContinueWith((w) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (AnyListBindable use in bindable)
                {
                    LastChats.Add(use);
                }
            });
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
        catch (Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }
}