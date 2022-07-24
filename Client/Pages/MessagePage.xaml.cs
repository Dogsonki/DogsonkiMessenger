using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace Client.Pages;

public partial class MessagePage : ContentPage
{
    private static MessagePage Current { get; set; }

    public static ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();

    private static User ChatUser { get; set; }
    private static Group GroupChat { get; set; }
    private static bool isGroupChat { get; set; } 

    private static void OnNewMessage(MessageModel LastMessage)
    {
        //Current.MessageList.ScrollTo(LastMessage, ScrollToPosition.End, false);
    }

    public MessagePage(User user)
    {
        ChatUser = user;
        isGroupChat = false;

        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        NavigationPage.SetHasBackButton(this, true);

        Current = this;
        Messages.Clear();

        ChatUsername.Text = "Chatting with @" + user.Username;
    }

    public MessagePage(Group group)
    {
        isGroupChat = true;
        GroupChat = group;

        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        Current = this;
        Messages.Clear();

        ChatUsername.Text = "Chatting group @" + group.Name;
    }

    protected override bool OnBackButtonPressed()
    {
        SocketCore.Send(" ", Token.END_CHAT);
        return base.OnBackButtonPressed();
    }

    public static void AddMessage(string message, DateTime time)
    {
        MessageModel u = new MessageModel(LocalUser.username, message, time);
        SocketCore.Send(message, Token.SEND_MESSAGE);
        Messages.Add(u);
        OnNewMessage(u);
    }

    public static void AddMessage(MessageModel message)
    {
        Messages.Add(message);
        OnNewMessage(message);
    }

    public static void AddMessage(SocketPacket packet)
    {
        if (!isGroupChat)
        {
            if (ChatUser.Id == LocalUser.Id) { return; }
        }

        MessageModel[] message = null;
        Task.Run(() =>
        {
            message = Essential.ModelCast<MessageModel[]>(packet.Data);
        }).ContinueWith((w) =>
        {
            foreach(MessageModel msg in message)
            {
                Debug.Write("MSG:: " + msg.MessageContent);
                if (!isGroupChat)
                {
                    if (msg.UserId != ChatUser.Id)
                    {
                        if (LocalUser.Id != msg.UserId)
                        {
                            return; //WARNING: check if all messages are from diffrent sources
                        }
                    }
                    if (msg.IsGroup)
                    {
                        continue;
                    }
                }
                else
                {
                    if (msg.GroupId != GroupChat.Id)
                    {
                        return;
                    }
                }
                MainThread.BeginInvokeOnMainThread(() => AddMessage(msg));
            }
        });
    }

    private void MessageInputDone(object sender, EventArgs e)
    {
        string message = ((Entry)sender).Text;

        if (string.IsNullOrEmpty(message))
            return;

        AddMessage(message, DateTime.Now);
        ((Entry)sender).Text = "";
    }

    public static void PrependNewMessages(object packet)
    {
        MessageModel[] messages = ((JArray)((SocketPacket)packet).Data).ToObject<MessageModel[]>();
        foreach(var msg in messages)
        {
            Messages.Insert(0, msg);
        }
       
        OnNewMessage(messages.Last());
    }

    private void Refresh(object sender, EventArgs e)
    {
        SocketCore.Send(" ", Token.GET_MORE_MESSAGES);
        ((ListView)sender).IsRefreshing = false;
    }

    private bool _isFoc = false;
    private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (MessageInput.Text.Length > 0 && !_isFoc)
        {
            _isFoc = true;
        }
        else if (MessageInput.Text.Length == 0 && _isFoc)
        {
            _isFoc = false;
        }
    }
}