using Client.Models.Bindable;
using Client.Networking.Core;
using System.Collections.ObjectModel;
using Client.Networking.Packets;
using Client.Networking.Packets.Models;
using Client.Utility;
using Newtonsoft.Json;
using Client.Models;

namespace Client.Pages.TemporaryPages.GroupChat;

public partial class GroupChatCreator : ContentPage
{
    public ObservableCollection<AnyListBindable> Invited { get; set; } = new ObservableCollection<AnyListBindable>();

    private List<AnyListBindable> _lastChats { get; init; }

    public ObservableCollection<AnyListBindable> InvitableChats { get; set; } = new ObservableCollection<AnyListBindable>(); 

    public GroupChatCreator(List<BindableLastChat> lastChats)
    {
        NavigationPage.SetHasNavigationBar(this, false);
        InitializeComponent();

        List<AnyListBindable> userLastChats = new List<AnyListBindable>();

        foreach(BindableLastChat chat in lastChats)
        {
            if (!chat.IsGroup)
            {
                IViewBindable? view = chat.view as User;

                if(view is not null)
                {
                    userLastChats.Add(new AnyListBindable(view));
                }
            }
        }

        _lastChats = userLastChats;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (AnyListBindable chat in userLastChats)
            {
                InvitableChats.Add(chat);
            }
        });
    }

    private void CreateGroupCallback(object data)
    {

    }

    private async void GroupImageChange(object? sender, EventArgs e)
    {

    }

    private void ShowInvited(object? sender, EventArgs e)
    {

    }

    private void ShowInvite(object? sender, EventArgs e)
    {

    }

    private void SearchPressed(object? sender, EventArgs e)
    {
        Entry? entry = (Entry?)sender;
        if (entry is null) return;

        if(string.IsNullOrEmpty(entry.Text) || entry.Text.Length < 1)
        {
            return;
        }

        SocketCore.SendCallback(new SearchPacket(entry.Text,false), Token.SEARCH_USER, SearchCallback);
    }

    private void SearchCallback(object packet)
    {

    }

    private void UserInvited(User user)
    {

    }

    private void UserRemovedFromInvited(User user)
    {

    }

    private void CreateGroupClicked(object? sender, EventArgs e)
    {

    }
}