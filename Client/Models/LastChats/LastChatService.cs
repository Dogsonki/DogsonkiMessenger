﻿using Client.Networking.Core;
using Client.Networking.Packets.Models;

namespace Client.Models.LastChats;

/// <summary>
/// Singelton service for last chats
/// </summary>
internal class LastChatService
{
    private readonly List<LastChat> lastChats = new List<LastChat>();

    public IEnumerable<LastChat> GetLastChats() => lastChats;

    public int ChatsCount => lastChats.Count;

    private readonly object padlock = new object();

    public void UpdateLastChatMessage(IViewBindable lastChat, IViewBindable lastMessageOwner, string newMessage, double time, MessageType messageType)
    {
        lock (padlock)
        {
            LastChat? lastChatRef = lastChats.Find(x => x.BindedView.Id == lastChat.Id);

            if (lastChatRef is not null)
            {
                lastChatRef.UpdateLastMessage(lastMessageOwner, newMessage, time, messageType);
            }
        }
    }

    public void AddLastChat(IViewBindable lastChatBind, IViewBindable lastMessageOwner, string message, double time, MessageType messageType)
    {
        lock (padlock)
        {
            if (lastChats.Find(x => x.View.Id == lastChatBind.Id) is not null)
            {
                return;
            }

            LastChat lastChat = new LastChat(lastChatBind, lastMessageOwner.Name, messageType, message, time, UserStatus.None);

            lastChats.Add(lastChat);
        }
    }

    public void AddLastChat(IViewBindable view, IViewBindable messageAuthor, MessageType messageType, string? message) 
    {
        lock(padlock) 
        {
            if (lastChats.Find(x => x.Id == view.Id) == null) {

                UserStatus status = UserStatus.None;

                if (view.BindType == BindableType.User) 
                {
                    status = ((User)view).UserProperties.Status;
                }

                LastChat lastChat = new LastChat(view, view.Name, messageType, message, 0, status);
                lastChats.Add(lastChat);
            }
        }
    }

    public void AddLastChat(LastChat lastChat)
    {
        lock (padlock)
        {
            if (lastChats.Find(x => x.View.Id == lastChat.Id) is not null)
            {
                return;
            }

            lastChats.Add(lastChat);
        }
    }

    public void FetchLastChats(Action<IEnumerable<LastChat>> lastChatsCallback) 
    {
        SocketCore.SendCallback(" ", Token.GET_LAST_CHATS, packet => 
        {
            LastChatsPacket[]? fetchedLastChats = packet.Deserialize<LastChatsPacket[]>();

            if(fetchedLastChats is null || fetchedLastChats.Length == 0) 
            {
                lastChatsCallback(new LastChat[0]);
                return;
            }

            foreach (LastChatsPacket lastChat in fetchedLastChats) {
                IViewBindable view = IViewBindable.CreateOrGet(lastChat.Name, lastChat.Id, lastChat.isGroup);

                if (view.IsUser()) 
                {
                    ((User)view).UserProperties.IsFriend = lastChat.IsFriend;
                }

                UserStatus status = UserStatus.None;

                if (lastChat.LastOnlineTime is not null) {
                    status = UserStatus.Online;
                }
                else {
                    status = UserStatus.Offline;
                }

                LastChat chat = new LastChat(view, lastChat.MessageSenderName, lastChat.TypeOfMessage,
                    lastChat.LastMessage, lastChat.LastMessageTime, status, lastChat.IsFriend);

                AddLastChat(chat);
            }

            lastChatsCallback(lastChats);
        });
    }
}