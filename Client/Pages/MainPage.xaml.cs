using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Pages.TemporaryPages.GroupChat;
using Client.Utility;
using System.Collections.ObjectModel;
using Client.Networking.Packets;
using Newtonsoft.Json;

namespace Client.Pages;

public partial class MainPage : ContentPage
{
    public static MainPage Current;
    public static ObservableCollection<AnyListBindable> LastChats { get; set; } = new ObservableCollection<AnyListBindable>();

    public MainPage()
    {
        InitializeComponent();

        if (Current is null) Current = this;

        NavigationPage.SetHasNavigationBar(this, false);

        SocketCore.SendCallback(AddLastChatsCallback, " ", Token.LAST_CHATS);
    }

    public static void AddLastChatsCallback(object packet)
    {
        List<AnyListBindable> bindable = new List<AnyListBindable>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SearchModel[]? lastChats = JsonConvert.DeserializeObject<SearchModel[]>((string)packet);

            if (lastChats is null || lastChats?.Length == 0)
            {
                Current.NoChatsMessage.IsVisible = true;
                Current.NoChatsMessage.IsEnabled = true;

                Current.CreateGroupButton.IsVisible = false;
                Current.CreateGroupButton.IsEnabled = false;
                return;
            }

            foreach (var chat in lastChats)
            {
                if (bindable.Find(x => x.Id == chat.Id) != null) continue;

                if(!chat.isGroup)
                {       
                    User user = User.CreateOrGet(chat.Username, chat.Id);
                    bindable.Add(new AnyListBindable(user,new Command(() => User.OpenChat(user))));
                }
                else
                {
                    Group group = Group.CreateOrGet(chat.Username, chat.Id);
                    bindable.Add(new AnyListBindable(group, new Command(() => Group.OpenChat(group))));
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

        await Navigation.PushAsync(new SearchPage(SearchInput));
    }
}