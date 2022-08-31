using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Pages.TemporaryPages.GroupChat;
using Client.Utility;
using System.Collections.ObjectModel;
using Client.Networking.Packets;

namespace Client.Pages;

public partial class MainPage : ContentPage
{
    public static ObservableCollection<AnyListBindable> LastChats { get; set; } = new ObservableCollection<AnyListBindable>();

    public MainPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        SocketCore.Send(" ", Token.LAST_CHATS);

        LastChats.Add(new AnyListBindable(User.CreateOrGet("michal",11),true));
    }

    public static void AddLastChats(SocketPacket packet)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            List<AnyListBindable> bindable = new List<AnyListBindable>();

            SearchModel[]? lastChats = Essential.ModelCast<SearchModel[]>(packet.Data);

            if (lastChats is null || lastChats.Length == 0)
                return;

            foreach (var chat in lastChats)
            {
                if (bindable.Find(x => x.Id == chat.Id) != null) continue;

                if(!chat.isGroup)
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

    private async void CreateGroup(object sender, EventArgs e) => await Navigation.PushAsync(new GroupChatCreator());
    private async void SettingsTapped(object sender, EventArgs e) => await Navigation.PushAsync(new SettingsPage());

    protected override bool OnBackButtonPressed()
    {
        view.ScrollToAsync(0d, 0d, true);
        return true;
    }

    private async void SearchPressed(object sender, EventArgs e)
    {
        string SearchInput = ((Entry)sender).Text;

        if (string.IsNullOrEmpty(SearchInput)) return;

        SocketCore.Send(SearchInput, Token.SEARCH_USER);
        await Navigation.PushAsync(new SearchPage(SearchInput));
    }
}