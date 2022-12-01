using Client.Networking.Core;
using Client.Utility;
using System.Collections.ObjectModel;
using Client.Models;
using Client.Models.Bindable;
using Client.Networking.Packets;
using Client.Pages.Settings;
using Newtonsoft.Json;
using Client.IO;
using Client.Networking.Commands;
using Client.Networking.Models;

namespace Client.Pages;

public partial class MessagePage : ContentPage
{
    private static MessagePage Current { get; set; }

    public static ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();

    //Contains ALL messages with their id 
    private static List<ChatMessage> _allMessages = new List<ChatMessage>();

    public static Conversation CurrentConversation { get; set; }
    private static bool isGroupChat { get; set; }
    private const int MAX_IMAGE_SIZE = 4_000_000;
    
    public MessagePage(User user)
    {
        CurrentConversation = new Conversation(user);

        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        Current = this;

        ChatUsername.Text = $"Chatting @{user.Username}";
        MessageInput.Placeholder = $"Message @{user.Username}";
    }

    public MessagePage(Group group)
    {
        CurrentConversation = new Conversation(group);

        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        Current = this;

        ChatUsername.Text = $"Chatting group @{group.Name}";
        MessageInput.Placeholder = $"Message @{group.Name}";
    }

    protected override bool OnBackButtonPressed()
    {
        SocketCore.Send(" ", Token.END_CHAT);

        ChatCache cache = new ChatCache(Messages.ToArray(), Conversation.Current.GetCurrentUserChat());

        CurrentConversation = null;
        Messages.Clear();

        return base.OnBackButtonPressed();
    }

    public void AddMessage(string message)
    {
        AddClientMessage(message);

        MessagePacket packet = new MessagePacket(message);

        SocketCore.Send(packet, Token.SEND_MESSAGE);

        if (message.StartsWith("!"))
        {
            Current.ProcessCommands(message.Split(" "));
        }
    }

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
                if (pickedFile.FileName.EndsWith(".png") || pickedFile.FileName.EndsWith(".jpg") || pickedFile.FileName.EndsWith(".jpeg"))
                {
                    fileStream = await pickedFile.OpenReadAsync();

                    if (fileStream.Length > MAX_IMAGE_SIZE)
                    {
                        await fileStream.DisposeAsync();
                        SystemAddMessage("Image is too big, max image size is 4MB");
                        return;
                    }

                    MemoryStream cpyStream = new MemoryStream();
                    await fileStream.CopyToAsync(cpyStream, (int)fileStream.Length);
                    ImageSource src = ImageSource.FromStream(() => new MemoryStream(cpyStream.ToArray()));

                    AddClientMessage(src);

                    ThreadPool.QueueUserWorkItem((padlock) =>
                    {
                        MessagePacket message = new MessagePacket(cpyStream.ToArray(), "jpeg");
                        SocketCore.Send(message, Token.SEND_MESSAGE);
                    });
                }
                else
                {
                    SystemAddMessage($"{pickedFile.FileName} Have unsupported file extension");
                }

                if(fileStream is not null) await fileStream.DisposeAsync();
            }
        }
        catch(Exception ex)
        {
            Logger.Push(ex, LogLevel.Error);
        }
    }

    private void ProcessCommands(string[] args)
    {
        try
        {
            string error = string.Empty;

            CommandProcess.Invoke(args[0], args, out error);

            if (error != string.Empty)
            {
                SystemAddMessage(error);
            } 
        }
        catch(Exception ex)
        {
            Debug.Error(ex);
        }
    }

    public static void SystemAddMessage(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Messages.Add(new ChatMessage(User.GetSystemBot(), message));
        });
    }

    private void AddClientMessage(string msg)
    {
        ChatMessage? lastMessage = GetLastMessage();

        if (lastMessage is null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Messages.Add(new ChatMessage(msg));
            });
        }
        else if (lastMessage.BindedUser.UserId == lastMessage.BindedUser.UserId && lastMessage.IsText)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lastMessage.TextContent += $"\n{msg}";
            });
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Messages.Add(new ChatMessage(msg));
            });
        }
    }

    private void AddClientMessage(ImageSource img)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Messages.Add(new ChatMessage(img));
        });
    }

    public void RealTimeMessageCallback(object packet)
    {
        MessagePacket? message = JsonConvert.DeserializeObject<MessagePacket>((string)packet);

        if (message is null)
        {
            throw new Exception("Realtime message is null");
        }

        ChatMessage? lastMessage = GetLastMessage();

        if (lastMessage is null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Messages.Add(new ChatMessage(message));
            });
        }
        else if(lastMessage.BindedUser.UserId == lastMessage.BindedUser.UserId && lastMessage.IsText)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lastMessage.TextContent += $"\n{message.ContentString}";
            });
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Messages.Add(new ChatMessage(message));
            });
        }
    }

    public void GetChatMessagesCallback(object packet)
    {
        try
        {
            List<MessagePacket>? messages = null;
            Task.Run(() =>
            {
                messages = JsonConvert.DeserializeObject<List<MessagePacket>>((string)packet);
            }).ContinueWith((w) =>
            {
                if (messages is null)
                {
                    Logger.Push($"Messages are null: {packet}", LogLevel.Error);
                    return;
                }

                if (messages.Count == 0)
                {
                    Logger.Push($"No messages: {packet}", LogLevel.Debug);
                    return;
                }

                if (messages.Count > 1)
                {
                    messages.Sort((x, y) => DateTime.Compare(x.Time, y.Time));
                }

                List<ChatMessage> messageView = new List<ChatMessage>();

                foreach (MessagePacket message in messages)
                {
                    ChatMessage? lastMessage = null;

                    if (messageView.Count > 0)
                    {
                        lastMessage = messageView.Last();
                    }

                    if (lastMessage is null)
                    {
                        messageView.Add(new ChatMessage(message, true, true));
                    }
                    else if (lastMessage.BindedUser.UserId == lastMessage.BindedUser.UserId && !lastMessage.IsImage)
                    {
                        lastMessage.TextContent += $"\n{message.ContentString}";
                    }
                    else
                    {
                        messageView.Add(new ChatMessage(message,true,true));
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var msg in messageView)
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

        input.Text = string.Empty;
    }

    public void GetMoreChatMessagesCallback(object packet)
    {
        MessagePacket[]? messages = JsonConvert.DeserializeObject<MessagePacket[]>((string)packet);
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
        if (CurrentConversation.IsGroupConversation) await Navigation.PushAsync(new GroupChatSettings());
        else await Navigation.PushAsync(new UserChatSettings());
    }
}