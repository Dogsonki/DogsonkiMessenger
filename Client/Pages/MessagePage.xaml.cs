using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;
using System.Collections.ObjectModel;
using Client.Commands;
using Client.Models;
using Client.Networking.Packets;
using Newtonsoft.Json.Linq;
using Client.Pages.Settings;
namespace Client.Pages;

public partial class MessagePage : ContentPage
{
    private static MessagePage Current { get; set; }

    public static ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();
    public static Conversation CurrentConversation { get; set; }

    private static bool isGroupChat { get; set; }
    private const int MAX_IMAGE_SIZE = 10_000_000;
    
    public MessagePage(User user)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        CurrentConversation = new Conversation(user);

        Current = this;

        ChatUsername.Text = $"Chatting @{user.Username}";
        MessageInput.Placeholder = $"Message @{user.Username}";
    }

    public MessagePage(Group group)
    {
        CurrentConversation = new Conversation(group);

        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        //TODO: get info only in settings page
        
        Current = this;

        ChatUsername.Text = $"Chatting group @{group.Name}";
        MessageInput.Placeholder = $"Message @{group.Name}";
    }

    protected override bool OnBackButtonPressed()
    {
        SocketCore.Send(" ", Token.END_CHAT);

        CurrentConversation = null;
        Messages.Clear();

        return base.OnBackButtonPressed();
    }

    public static void AddMessage(string message)
    {
        ChatMessage? lastMessage = GetLastMessage();

        MessagePacket packet = new MessagePacket(message);
        SocketCore.Send(packet, Token.SEND_MESSAGE);

        if (IsLastMessageFromLocalUser && lastMessage is not null && !lastMessage.IsImage)
        {
            lastMessage.TextContent += $"\n{message}";
        }
        else
        {
            ChatMessage PreparedMessage = new ChatMessage(message);

            Messages.Add(PreparedMessage);
        }

        if (message.StartsWith("!"))
        {
            Current.ProcessCommands(message.Split(" "));
        }
    }

    private static void AddImageMessage(ImageSource src) => Messages.Add(new ChatMessage(src));

    private static bool IsLastMessageFromLocalUser => GetLastMessage()?.BindedUser.UserId == LocalUser.Id;

    private static ChatMessage? GetLastMessage()
    {
        if (Messages.Count > 0)
        {
            return Messages.Last();
        }
        return null;
    }

    private async void AddFile(object sender, EventArgs e)
    {
        //Android had problem with copying stream to MemoryStream 
        //TODO: redo this without copying buffer 2 times
        try
        {
            var pickedFile = await FilePicker.PickAsync();
            Stream? fileStream = null;

            if (pickedFile is not null)
            {
                if (pickedFile.FileName.EndsWith(".png") || pickedFile.FileName.EndsWith(".jpg"))
                {
                    fileStream = await pickedFile.OpenReadAsync();
                    if (fileStream.Length > MAX_IMAGE_SIZE)
                    {
                        await fileStream.DisposeAsync();
                        SystemAddMessage("Image is too big, max image size is 10MB");
                    }

                    MemoryStream cpyStream = new MemoryStream();
                    await fileStream.CopyToAsync(cpyStream, (int)fileStream.Length);
                    ImageSource src = ImageSource.FromStream(() => new MemoryStream(cpyStream.ToArray()));

                    AddImageMessage(src);

                    ThreadPool.QueueUserWorkItem((padlock) =>
                    {
                        MessagePacket message = new MessagePacket(cpyStream.ToArray(), "jpeg");
                        SocketCore.Send(message, Token.SEND_MESSAGE);
                    });
                }
                else
                {
                    SystemAddMessage("Unsupported file extension");
                }

                if(fileStream is not null) await fileStream.DisposeAsync();
            }
        }
        catch(Exception ex)
        {
            Logger.Push(ex, TraceType.Func, LogLevel.Error);
        }
    }

    private void ProcessCommands(string[] args)
    {
        try
        {
            string command = args[0];
            string error = string.Empty;

            switch (command)
            {
                case "!daily":
                    ICommand.PrepareAndSend(new Daily(command), out error);
                    break;
                case "!bet":
                    if (!Bet.HasArgs(args.Length)) return;
                    ICommand.PrepareAndSend(new Bet(command, args[1], args[2]), out error);
                    break;
                case "!jackpotbuy":
                    if (!JackpotBuy.HasArgs(args.Length)) return;
                    ICommand.PrepareAndSend(new JackpotBuy(command, args[1]), out error);
                    break;
                case "!zdrapka":
                    SocketCore.SendCommand(new Scratchcard(command));
                    break;
                case "!sklep":
                    if (!Shop.HasArgs(args.Length)) return;
                    ICommand.PrepareAndSend(new Shop(command, args[1]), out error);
                    break;
                case "!slots":
                    SocketCore.SendCommand(new Slots(command));
                    break;
                case "!clear":
                    Messages.Clear();
                    break;
                /*
                case "!invite":
                    SocketCore.Send(new GroupChatUserInvitePacket(GroupChat.Id, int.Parse(args[1])),Token.GROUP_INVITE);
                    break;
                case "!remove":
                    SocketCore.Send(new GroupChatUserRemovePacket(GroupChat.Id, int.Parse(args[1])), Token.GROUP_USER_KICK);
                    break;
                */
            }
            if(error != string.Empty)
            {
                SystemAddMessage(error);
            } 
        }
        catch(Exception ex)
        {
            Debug.Error(ex);
        }
    }

    public static void SystemAddMessage(string message) => Messages.Add(new ChatMessage(User.GetSystemBot(), message));

    public static void AddMessage(ChatMessage message)
    {
        ChatMessage? lastMessage = GetLastMessage();

        if(lastMessage is not null)
        {
            if(lastMessage.BindedUser.UserId == message.BindedUser.UserId)
            {
                lastMessage.TextContent += $"\n{message.TextContent}";
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Messages.Add(message);
                });
            }
        }
        else
        {
            Messages.Add(message);
        }
    }

    public static void AddMessage(SocketPacket packet)
    {
        try
        {
            List<MessagePacket>? messages = null;
            Task.Run(() =>
            {
                JArray r = (JArray)packet.Data;
                messages = r.ToObject<List<MessagePacket>>();
            }).ContinueWith((w) =>
            {
                if (messages is null)
                {
                    Logger.Push($"Messages are null: {packet.Data}", TraceType.Func, LogLevel.Error);
                    return;
                }

                if (messages.Count == 0)
                {
                    Logger.Push($"No messages: {packet.Data}", TraceType.Func, LogLevel.Debug);
                    return;
                }


                messages.Sort((x, y) => DateTime.Compare(x.Time, y.Time));

                List<ChatMessage> addedMessages = new List<ChatMessage>(messages.Count);

                foreach (MessagePacket message in messages)
                {
                    ChatMessage? lastMessage = null;

                    if (addedMessages.Count > 0)
                    {
                        lastMessage = addedMessages.Last();
                    }


                    object _padlock = new object();

                    lock (_padlock)
                    {
                        if (lastMessage is not null && lastMessage.BindedUser.UserId == message.UserId && lastMessage.IsText && message.MessageType == "text")
                        {
                            lastMessage.textContent += $"\n{message.ContentString}";
                        }
                        else
                        {
                            addedMessages.Add(new ChatMessage(message));
                        }
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (ChatMessage msg in addedMessages)
                    {
                        Messages.Add(msg);
                    }
                });
            });
        }
        catch (Exception ex)
        {
            Debug.Write(ex);
        }
    }

    public static void AddMessage(MessagePacket packet) => Messages.Add(new ChatMessage(packet));

    private void MessageInputDone(object sender, EventArgs e)
    {
        Entry input = (Entry)sender;
        string message = input.Text;

        if (string.IsNullOrEmpty(message))
            return;

        AddMessage(message);

        input.Text = "";
    }

    public static void PrependNewMessages(SocketPacket packet)
    {
        MessagePacket[]? messages = packet.Data.ModelCast<MessagePacket[]>();
        if (messages is null) return;
    }

    private void Refresh(object sender, EventArgs e)
    {
        SocketCore.Send(" ", Token.GET_MORE_MESSAGES);
        ((ListView)sender).IsRefreshing = false;
    }

    private int _lastMessageLen;

    private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (MessageInput.Text.StartsWith("!"))
        {
            if (!CommandsList.IsEnabled)
            {
                CommandsList.IsEnabled = true;
                CommandsList.IsVisible = true;
                CommandList.Commands.ReplaceRange(CommandList.AllCommands);
            }
            else
            {
                if (CommandList.Commands.Count > 1 && _lastMessageLen <= MessageInput.Text.Length)
                {
                    CommandList.Commands.ReplaceRange(CommandList.AllCommands.Where(x => x.CommandName.StartsWith(MessageInput.Text)));
                }
                else if (_lastMessageLen > MessageInput.Text.Length)
                {
                    var _ = CommandList.AllCommands.Where(x => x.CommandName.StartsWith(MessageInput.Text));
                    if(_.Count() > 0)
                    {
                        CommandList.Commands.ReplaceRange(CommandList.AllCommands.Where(x => x.CommandName.StartsWith(MessageInput.Text)));
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

    private async void RedirectToChatSettings(object? sender, EventArgs e)
    {
        if (CurrentConversation.IsGroupConversation)
        {
            await Navigation.PushAsync(new GroupChatSettings());
        }
        else
        {
            await Navigation.PushAsync(new UserChatSettings());
        }
    }
}