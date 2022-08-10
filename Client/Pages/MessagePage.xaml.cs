using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Client.Pages.TemporaryPages.ChatOptions;
using Client.Networking.Models.BotCommands;
using System.Linq;
using System.Windows.Input;

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
        Current.MessageList.ScrollTo(LastMessage, ScrollToPosition.End, false);
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

        ChatUsername.Text = "@" + user.Username;
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

    public static void AddMessage(string message)
    {
        MessageModel? lastMessage = GetLastMessage();

        MessageModel PreparedMessage = new MessageModel(message);
        SocketCore.Send(message, Token.SEND_MESSAGE);

        if (IsLastMessageFromLocalUser && lastMessage is not null && !lastMessage.IsImageMessage)
        {
            lastMessage.MessageContent += $"\n{message}";
        }
        else
        {
            Messages.Add(PreparedMessage);
        }

        if (message.StartsWith("!"))
        {
            ProcessCommands(message.Split(" "));
        }
    }

    private static bool IsLastMessageFromLocalUser => GetLastMessage()?.UserId == LocalUser.Id;

    private static MessageModel? GetLastMessage()
    {
        if (Messages.Count > 0)
        {
            return Messages.Last();
        }
        else
        {
            return null;
        }
    }

    private static void AddImageMessage(ImageSource src)
    {
        Messages.Add(new MessageModel(src));
    }

    private async void GetFile(object sender, EventArgs e)
    {
        var pickedFile = await FilePicker.PickAsync();
        if (pickedFile is not null)
        {
            if(pickedFile.FileName.EndsWith(".png") || pickedFile.FileName.EndsWith(".jpg"))
            {
                var imageMemoryStream = await pickedFile.OpenReadAsync();
                ImageSource src = ImageSource.FromStream(() => imageMemoryStream);
                AddImageMessage(src);
            }
        }
    }
    private static void ProcessCommands(string[] args)
    {
        try
        {
            string command = args[0];
            string error = string.Empty;

            switch (command)
            {
                case "!daily":
                    SocketCore.SendCommand(new Daily(command));
                    if(!IBotCommand.PrepareAndSend(new Daily(command), out error))
                    {
                        AddMessage(error);
                    }
                    break;
                case "!bet":
                    if (!Bet.HasArgs(args.Length)) return;
                    if(!IBotCommand.PrepareAndSend(new Bet(command, args[1], args[2]),out error))
                    {
                        AddMessage(error);
                    }
                    break;
                case "!jackpotbuy":
                    if (!JackpotBuy.HasArgs(args.Length)) return;
                    if (!IBotCommand.PrepareAndSend(new JackpotBuy(command, args[1]), out error))
                    {
                        AddMessage(error);
                    }
                    break;
                case "!zdrapka":
                    SocketCore.SendCommand(new Scratchcard(command));
                    break;
                case "!sklep":
                    if (!Shop.HasArgs(args.Length)) return;
                    if (!IBotCommand.PrepareAndSend(new Shop(command, args[1]), out error))
                    {
                        AddMessage(error);
                    }
                    break;
                case "!slots":
                    SocketCore.SendCommand(new Slots(command));
                    break;
                case "!clear":
                    Messages.Clear();
                    break;
            }
        }
        catch(Exception ex)
        {
            Debug.Error(ex);
        }
    }

    public static void AddMessage(MessageModel message)
    {
        MessageModel? lastMessage = GetLastMessage();
        if(lastMessage is not null)
        {
            if(lastMessage.UserId == message.UserId)
            {
                lastMessage.MessageContent += $"\n{message.MessageContent}";
            }
            else
            {
                Messages.Add(message);
            }
        }
        else
        {
            Messages.Add(message);
        }
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

        AddMessage(message);
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
    private int _lastMessageLen;

    private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (MessageInput.Text.StartsWith("!"))
        {
            if (!CommandsList.IsEnabled)
            {
                CommandsList.IsEnabled = true;
                CommandsList.IsVisible = true;
                BotCommandList.Commands.ReplaceRange(BotCommandList.AllCommands);
            }
            else
            {
                if (BotCommandList.Commands.Count > 1 && _lastMessageLen <= MessageInput.Text.Length)
                {
                    BotCommandList.Commands.ReplaceRange(BotCommandList.AllCommands.Where(x => x.CommandName.StartsWith(MessageInput.Text)));
                }
                else if (_lastMessageLen > MessageInput.Text.Length)
                {
                    var _ = BotCommandList.AllCommands.Where(x => x.CommandName.StartsWith(MessageInput.Text));
                    if(_.Count() > 0)
                    {
                        BotCommandList.Commands.ReplaceRange(BotCommandList.AllCommands.Where(x => x.CommandName.StartsWith(MessageInput.Text)));
                    }
                }
            }
        }
        else
        {
            CommandsList.IsEnabled = false;
            CommandsList.IsVisible = false;
        }
        _lastMessageLen = MessageInput.Text.Length;
    }

    private async void ChatOptions(object sender, SwipedEventArgs e)
    {
        Debug.Write("aa");
        await Navigation.PushAsync(new ChatOptionsMain(), true);
    }

    private void DebugSelected(object sender, SelectedItemChangedEventArgs e)
    {
        MessageModel msg = (MessageModel)e.SelectedItem;
        Debug.Write(msg.Username);
        Debug.Write(msg.AvatarImage.Id);
    }
}