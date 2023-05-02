using Client.IO;
using Client.Models;
using Client.Models.LastChats;
using Client.Models.Navigation;
using Client.Networking.Commands;
using Client.Networking.Core;
using Client.Networking.Models;
using Client.Networking.Packets;
using Client.Pages.Exceptions;
using Client.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Client.Models.Chat;

namespace Client.Pages;

public partial class ChatPage 
{
    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public bool IsGroup { get; set; }

    private ElementReference MessageFrame;

    private string ChatName { get; set; } = string.Empty;

    private IViewBindable View;

    public readonly static List<ChatMessage> Messages = new List<ChatMessage>();

    private string? MessageInputContent;

    public ChatPage() 
    {
        Messages.Clear();
    }

    protected override void OnAfterRender(bool firstRender) {
        if(firstRender) {
            LocalUser.CurrentUser.SetPropertyChanged(InvokeAsync(StateHasChanged), true);
        }

        base.OnAfterRender(firstRender);
    }

    protected override void OnParametersSet()
    {
        InitChat();
    }

    private void InitChat() 
    {
        View = IViewBindable.Get((uint)Id, IsGroup);

        if (View is null) {
            throw new WrongParameterException("There is no pre-created view with given Id");
        }

        SocketCore.OnToken(Token.CHAT_MESSAGE, OnReceiveRealtimeMessage);
        SocketCore.OnToken(Token.GET_MORE_MESSAGES, OnGetMoreMessages);

        SocketCore.SendCallback(" ", Token.GET_INIT_MESSAGES, InitMessages, false);

        LocalUser.CurrentUser.UserProperties.IsChatting = true;

        ChatName = View.Name;

        navigation.LocationChanged += OnBackButtonClicked;
    }

    private void OnBackButtonClicked(object? sender, LocationChangedEventArgs e)
    {
        if (e.GetPageName() == "/MainPage" && Conversation.IsLocalUserInChat)
        {
            Conversation.CloseChat();
        }
    }

    private void InitMessages(SocketPacket packet)
    {
        List<MessagePacket>? initMessages = packet.ModelCast<List<MessagePacket>>();

        if(initMessages is null || initMessages.Count == 0)
        {
            return;
        }

        initMessages.Reverse();

        foreach (MessagePacket initMessage in initMessages)
        {
            ChatMessage chatMessage = null;

            if (initMessage.IsBot) {
                chatMessage = new ChatMessage(initMessage.ContentString, initMessage.IsImage,
                initMessage.MessageId, StateChanged, 0, initMessage.Time, initMessage.IsBot);
            }
            else {
                chatMessage = new ChatMessage(initMessage.ContentString, initMessage.IsImage,
                initMessage.MessageId, StateChanged, (int)initMessage.UserId, initMessage.Time, initMessage.IsBot);
            }

            AddMessage(chatMessage, initMessage.MessageId);
        }

        InvokeAsync(StateHasChanged);
    }

    private void OnGetMoreMessages(SocketPacket packet) {

    }

    private void OnReceiveRealtimeMessage(SocketPacket packet) 
    {
        MessagePacket? message = packet.Deserialize<MessagePacket>();

        if (message is null || message.UserId == LocalUser.CurrentUser.Id) 
        {
            return;
        }

        ChatMessage chatMessage;

        if (message.IsBot) {
            chatMessage = new ChatMessage(message.ContentString, message.IsImage,
            message.MessageId, StateChanged, 0, message.Time, message.IsBot);
        }
        else {
            chatMessage = new ChatMessage(message.ContentString, message.IsImage,
            message.MessageId, StateChanged, (int)message.UserId, message.Time, message.IsBot );
        }

        AddMessage(chatMessage);

        InvokeAsync(StateHasChanged);
    }

    private void MessageInputSubmit()
    {
        if (string.IsNullOrEmpty(MessageInputContent)) return;

        lastChatService.AddLastChat(View, LocalUser.CurrentUser, MessageType.Text, MessageInputContent);

        AddMessage(MessageInputContent);

        MessageInputContent = string.Empty;
    }

    private bool IsFileImage(string extension)
    {
        switch (extension)
        {
            case "jpg": return true;
            case "png": return true;
            case "gif": return true;
            case "svg": return true;
            case "jpeg": return true;
        }

        return false;
    }

    private async void AddFileMessage()
    {
        FileResult? selectedFile = await FilePicker.PickAsync();

        if (selectedFile is null)
        {
            return;
        }

        using (Stream stream = await selectedFile.OpenReadAsync())
        {
            byte[] streamBuffer = Essential.StreamToBuffer(stream);

            string extension = selectedFile.FileName.Substring(selectedFile.FileName.Length - 3);

            ChatMessage message;

            if (IsFileImage(extension))
            {
                string jsSource = AvatarManager.ToJSImageSource(streamBuffer);
                message = new ChatMessage(jsSource, MessageType.Image, StateChanged);
            }
            else
            {
                string jsSource = AvatarManager.ToJSVideoSource(streamBuffer);
                message = new ChatMessage(jsSource, MessageType.Video, StateChanged, extension);
            }

            AddMessage(message);
            SendFile(streamBuffer, extension);
        }
    }

    private void AddMessage(ChatMessage message, int messageId = 0)
    {
        ChatMessage? lastMessage = Messages.LastOrDefault();

        if (lastMessage is null)
        {
            Messages.Add(message);
        }
        else if (lastMessage.AuthorView.Id == message.AuthorView.Id)
        {
            lastMessage.Append(message.ChatMessageBodies[0].Content, message.ChatMessageBodies[0].type, messageId);
        }
        else
        {
            Messages.Add(message);
        }
    }

    private void SendFile(byte[] imageBuffer, string extension)
    {
        MessagePacket message = new MessagePacket(imageBuffer, extension);
        SocketCore.Send(message, Token.SEND_MESSAGE);
    }

    private void AddMessage(string content)
    {
        if (content.StartsWith("!")) 
        {
            string[] commandArguments = content.Split(' ');
            string commandName = commandArguments[0];

            if(!CommandProcess.Invoke(commandName, commandArguments, out string commandError))
            {
                ChatMessage message = new ChatMessage(commandError, MessageType.Text, StateChanged);
                AddMessage(message);
            }
        }
        else 
        {
            MessagePacket messagePacket = new MessagePacket(content);

            SocketCore.Send(messagePacket, Token.SEND_MESSAGE);

            ChatMessage message = new ChatMessage(content, MessageType.Text, StateChanged);

            AddMessage(message);
        }
    }

    private void StateChanged()
    {
        InvokeAsync(StateHasChanged);
        JS.InvokeVoidAsync("ScrollToBottom", new object[] { MessageFrame });
    }
}