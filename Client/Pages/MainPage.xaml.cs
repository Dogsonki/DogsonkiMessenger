using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Pages.TemporaryPages.GroupChat;
using System.Collections.ObjectModel;
using Client.Networking.Packets.Models;
using Newtonsoft.Json;

namespace Client.Pages;

public partial class MainPage : ContentPage
{
    public static MainPage Current;
    public static ObservableCollection<BindableLastChat> LastChats { get; set; } = new ObservableCollection<BindableLastChat>();

    public MainPage()
    {
        InitializeComponent();

        if (Current is null) Current = this;

        NavigationPage.SetHasNavigationBar(this, false);

        SocketCore.SendCallback(AddLastChatsCallback," ", Token.LAST_CHATS);
    }

    public static void AddLastChat(User user)
    {
        /*
        if (LastChats.Count > 0)
        {
            if(!LastChats.Any(x => x.Id == user.UserId && !x.IsGroup))
            {
                MainThread.BeginInvokeOnMainThread(() => LastChats.Add(new AnyListBindable(user, new Command(() => User.OpenChat(user)))));
            }
        }
        */
    }

    public static void AddLastChat(Group group)
    {
        /*
        if (LastChats.Count > 0)
        {
            if (!LastChats.Any(x => x.Id == group.Id && x.IsGroup))
            {
                MainThread.BeginInvokeOnMainThread(() => LastChats.Add(new AnyListBindable(group, new Command(() => Group.OpenChat(group)))));
            }
        }
        */
    }


    public static void AddLastChatsCallback(object packet)
    {
        List<BindableLastChat> bindable = new List<BindableLastChat>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            LastChatsPacket[]? lastChats = JsonConvert.DeserializeObject<LastChatsPacket[]>((string)packet);

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
                BindableLastChat bindableLastChat = new BindableLastChat(chat.Name, chat.Type,
                    chat.LastMessage, chat.LastMessageTime, chat.MessageType, chat.Id, chat.Sender);

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