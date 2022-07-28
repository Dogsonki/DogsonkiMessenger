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

    public MainPage()
    {
        InitializeComponent();

        NavigationPage.SetHasNavigationBar(this, false);
        SocketCore.Send(" ", Token.LAST_CHATS);

        if (Instance is null)
        {
            Instance = this;

            Logger.Push("Main page initialized", TraceType.Func, LogLevel.Debug);
        }

#if DEBUG
        LastChats.Add(new AnyListBindable(User.CreateOrGet("uwu", 11),true));
#endif
    }

    public static void AddLastChats(SocketPacket packet)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            List<AnyListBindable> bindable = new List<AnyListBindable>();

            SearchModel[] lastChats = Essential.ModelCast<SearchModel[]>(packet.Data);

            foreach (var chat in lastChats)
            {
                if(chat.Type == "user")
                {       
                    User user = User.CreateOrGet(chat.Username, chat.Id);
                    bindable.Add(new AnyListBindable(user,true));
                }
                else
                {
                    Group group = Group.CreateOrGet(chat.Username, chat.Id);
                    bindable.Add(new AnyListBindable(group,true));
                }
            }

            foreach (AnyListBindable bind in bindable)
            {
                LastChats.Add(bind);
            }
        });
    }

    protected override bool OnBackButtonPressed()
    {
        view.ScrollToAsync(0d, 0d, true);
        return true;
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