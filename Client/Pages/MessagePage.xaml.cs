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

    public static User ChatUser { get; set; }

    private static void OnNewMessage(MessageModel LastMessage)
    {
        //Current.MessageList.ScrollTo(LastMessage, ScrollToPosition.End, false);
    }

    public MessagePage(User user)
    {
        ChatUser = user;

        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        NavigationPage.SetHasBackButton(this, true);

        Current = this;
        Messages.Clear();

        ChatUsername.Text = "Chatting with @" + user.Username;
    }

    //TODO: THIS IS FUCK BUGGED 
    protected override bool OnBackButtonPressed()
    {
        Logger.Push("CHAT_CLOSE " + ChatUser.Username + "" + ChatUser.Id, TraceType.Func, LogLevel.Debug);
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
        if (ChatUser.Id == int.Parse(LocalUser.id)) { return; }

        Debug.Write("msg: "+packet.Data.GetType());

        MessageModel[] message = null;
        Task.Run(() =>
        {
            message = Essential.ModelCast<MessageModel[]>(packet.Data);
        }).ContinueWith((w) =>
        {
            foreach(MessageModel msg in message)
            {
                if (msg.UserId != ChatUser.Id)
                {
                    if (int.Parse(LocalUser.id) != msg.UserId)
                    {
                        continue;
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
            //MessageInputFrame.Animate("WidthRequest", animation: new Animation(callback: (double d) => { MessageInputFrame.WidthRequest = d; }, start: 200, end: 350, easing: Easing.Linear), length: 250);
            _isFoc = true;
        }
        else if (MessageInput.Text.Length == 0 && _isFoc)
        {
            //MessageInputFrame.Animate("WidthRequest", animation: new Animation(callback: (double d) => { MessageInputFrame.WidthRequest = d; }, start: 350, end: 200, easing: Easing.SpringIn), length: 500);
            _isFoc = false;
        }
    }
}