using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;
using System.Collections.ObjectModel;
using Client.Pages.TemporaryPages.ChatOptions;
using Client.Commands;
using Client.Networking.Packets;
using Client.Networking.Packets.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client.Pages;

public partial class MessagePage : ContentPage
{
    private static MessagePage Current { get; set; }

    public static ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();

    private static User ChatUser { get; set; }
    private static Group GroupChat { get; set; }
    private static bool isGroupChat { get; set; }
    private const int MAX_IMAGE_SIZE = 10_000_000;
    
    public MessagePage(User user)
    {
        try
        {
            ChatUser = user;
            isGroupChat = false;

            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            Current = this;

            ChatUsername.Text = $"@{user.Username}";
            MessageInput.Placeholder = $"Message @{user.Username}";
        }
        catch (Exception ex)
        {
            SocketCore.Send(ex, Token.EMPTY);
        }
    }

    public MessagePage(Group group)
    {
        GroupChat = group;
        isGroupChat = true;

        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        SocketCore.SendCallback(GroupChatInfoCallback, " ", Token.GET_GROUP_INFO, false);

        Current = this;

        ChatUsername.Text = $"Chatting group @{group.Name}";
        MessageInput.Placeholder = $"Message @{group.Name}";
    }

    private void GroupChatInfoCallback(object data)
    {
        GroupChatUserInfo[]? users = JsonConvert.DeserializeObject<GroupChatUserInfo[]>((string)data);

        if (users is null)
        {
            throw new Exception("GROUP_CHAT_USERS_INFO_NULL");
        }

        foreach (var u in users)
        {
            User user = User.CreateOrGet(u.UserName,u.UserId);

            GroupUser gu = new GroupUser(u.IsAdmin, user);

            GroupChat.AddUser(gu);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        SocketCore.Send(" ", Token.END_CHAT);
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
            ProcessCommands(message.Split(" "));
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
    private static void ProcessCommands(string[] args)
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
                case "!invite":
                    SocketCore.Send(new GroupInvitePacket(GroupChat.Id, int.Parse(args[1])));
                    break;
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
        if (!isGroupChat && ChatUser.UserId == LocalUser.Id ) { return; }

        List<MessagePacket>? messages = null;
        Task.Run(() =>
        {
            JArray r = (JArray)packet.Data;
            messages = r.ToObject<List<MessagePacket>>();
        }).ContinueWith((w) =>
        {
            if (messages is null)
            {
                Logger.Push($"Messages are null: {packet.Data}",TraceType.Func,LogLevel.Error);
                return;
            }
            else if (messages.Count == 0)
            {
                Logger.Push($"No messages: {packet.Data}",TraceType.Func,LogLevel.Debug);
                return;
            }

            messages.Sort((x, y) => DateTime.Compare(x.Time,y.Time));

            foreach(MessagePacket message in messages)
            {
                if (!isGroupChat)
                {
                    if (message.UserId != ChatUser.UserId && LocalUser.Id != message.UserId)
                    {
                        return;
                    }
                    else
                    {
                        //TODO: Notification 
                    }
                }
                else if(message.GroupId != GroupChat.Id)
                {
                    return;
                }

                ChatMessage? lastMessage = GetLastMessage();

                if (lastMessage is not null && lastMessage.BindedUser.UserId == message.UserId && lastMessage.IsText)
                {
                    MainThread.BeginInvokeOnMainThread(() => lastMessage.textContent += message.ContentString);
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() => AddMessage(message));
                }
            }
        });
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

    private async void ChatOptions(object sender, SwipedEventArgs e) => await Navigation.PushAsync(new ChatOptionsMain(), true);
}