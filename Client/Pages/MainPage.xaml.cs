using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Pages.TemporaryPages.GroupChat;
using System.Collections.ObjectModel;
using Client.Networking.Packets.Models;
using Newtonsoft.Json;
using Client.Pages.Settings;
using Client.Networking.Models;

namespace Client.Pages;

public partial class MainPage : ContentPage
{
    public static MainPage Current;
    public ObservableCollection<BindableLastChat> LastChats { get; set; } = new ObservableCollection<BindableLastChat>();

    public MainPage()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(LastChatsView, LastChats);
        if (Current is null) Current = this;

        NavigationPage.SetHasNavigationBar(this, false);

        SocketCore.SendCallback(" ", Token.LAST_CHATS, AddLastChatsCallback);
    }

    public void AddLastChatsCallback(object packet)
    {
        List<BindableLastChat> bindable = new List<BindableLastChat>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            LastChatsPacket[]? lastChats = JsonConvert.DeserializeObject<LastChatsPacket[]>(Convert.ToString(packet));

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
                BindableLastChat bindableLastChat = new BindableLastChat(chat.Name, chat.Type, chat.LastMessage, chat.LastMessageTime, chat.MessageType, chat.Id, chat.Sender);

                if (!chat.isGroup)
                {       
                    User user = User.CreateOrGet(chat.Name, chat.Id);
                    bindable.Add(bindableLastChat);
                }
                else
                {
                    Group group = Group.CreateOrGet(chat.Name, chat.Id);
                    bindable.Add(bindableLastChat);
                }
            }

            foreach (BindableLastChat bind in bindable)
            {
                LastChats.Add(bind);
            }
        });
    }

    private async void CreateGroup(object sender, EventArgs e) => await Navigation.PushAsync(new GroupChatCreator());
    private async void SettingsTapped(object sender, EventArgs e) => await Navigation.PushAsync(new SettingsPage());
    private async void ProfileImageTapped(object sender, EventArgs e) => await Navigation.PushAsync(new ProfileSettings());

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