using Client.IO;
using Client.Models;
using Client.Models.Navigation;
using Client.Networking.Commands;
using Client.Networking.Core;
using Client.Networking.Models;
using Client.Networking.Packets;
using Client.Pages.Exceptions;
using Client.Utility;
using Clinet.IO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

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

    private List<ChatMessage> Messages = new List<ChatMessage>();

    private string? MessageInputContent;

    protected override void OnParametersSet()
    {
        View = IViewBindable.Get((uint)Id, IsGroup);

        if(View is null)
        {
            throw new WrongParameterException("There is no pre-created view with given Id");
        }

        SocketCore.OnToken(Token.CHAT_MESSAGE, OnReceiveRealtimeMessage);
        SocketCore.OnToken(Token.GET_MORE_MESSAGES, OnGetMoreMessages);

        SocketCore.SendCallback(" ", Token.GET_INIT_MESSAGES, InitMessages, false);

        ChatName = View.Name;

        navigation.LocationChanged += OnBackButtonClicked;
    }

    private void OnBackButtonClicked(object? sender, LocationChangedEventArgs e)
    {
        if (e.GetPageName() == "/MainPage" && Conversation.IsLocalUserInChat)
        {
            Conversation.CloseChat();
            SocketCore.SendCallback(" ", Token.END_CHAT, null, true);
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
            ChatMessage message = new ChatMessage(initMessage.ContentString, initMessage.IsImage,
                initMessage.MessageId, StateChanged, initMessage.UserId, initMessage.Time);
            AddMessage(message);
        }

        InvokeAsync(StateHasChanged);
    }

    private void OnGetMoreMessages(SocketPacket packet)
    {

    }

    private void OnReceiveRealtimeMessage(SocketPacket packet)
    {
        MessagePacket? message = packet.Deserialize<MessagePacket>();

        if(message is null) 
        {
            return;
        }

        ChatMessage chatMessage = new ChatMessage(message.ContentString, message.IsImage, 
            message.MessageId, StateChanged, message.UserId, message.Time, message.IsBot);

        AddMessage(chatMessage);
    }

    private void MessageInputSubmit()
    {
        if (string.IsNullOrEmpty(MessageInputContent)) return;

        AddMessage(MessageInputContent);

        MessageInputContent = string.Empty;
    }

    private async void AddFileMessage()
    {
        byte[] imageSelected = await FileManager.FileFromSelectedFile();

        string jsSource = AvatarManager.ToJSImageSource(imageSelected);

        ChatMessage message = new ChatMessage(jsSource, true, StateChanged);

        AddMessage(message);

        SendImage(imageSelected, "jpeg");
    }

    private void AddMessage(ChatMessage message)
    {
        ChatMessage? lastMessage = Messages.LastOrDefault();

        if (lastMessage is null)
        {
            Messages.Add(message);
        }
        else if (lastMessage.AuthorView.Id == message.AuthorView.Id)
        {
            lastMessage.Append(message.ChatMessageBodies[0].Content, message.ChatMessageBodies[0].IsText != true);
        }
        else
        {
            Messages.Add(message);
        }
    }

    private void SendImage(byte[] imageBuffer, string extension)
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
                ChatMessage message = new ChatMessage(commandError, false, null);
                AddMessage(message);
            }
        }
        else 
        {
            MessagePacket messagePacket = new MessagePacket(content);

            SocketCore.Send(messagePacket, Token.SEND_MESSAGE);

            ChatMessage message = new ChatMessage(content, isImage: false, StateChanged);

            AddMessage(message);
        }
    }

    private void ProcessCommand(string command) 
    {
        
    }

    private void StateChanged()
    {
        InvokeAsync(StateHasChanged);
        JS.InvokeVoidAsync("ScrollToBottom", new object[] { MessageFrame });
    }
}