using Client.Networking.Core;
using Client.Networking.Models;
using Client.Utility;

namespace Client.Models.Chat;

public static class ChatMessageQueue
{
    private static Queue<ChatMessageBody> QueuedMessages { get; } = new Queue<ChatMessageBody>();

    static ChatMessageQueue()
    {
        SocketCore.OnToken(Token.GET_LAST_MESSAGE_ID, OnMessageIdGet);
    }

    public static void EnQueue(ChatMessageBody message)
    {
        if (message.MessageId != -1)
        {
            Logger.Push("Message body already has messageId", LogLevel.Warning);
            return;
        }

        if (QueuedMessages.Contains(message))
        {
            Logger.Push("MessageBody is already queued", LogLevel.Warning);
            return;
        }


        QueuedMessages.Enqueue(message);
    }

    private static void OnMessageIdGet(SocketPacket packet)
    {
        int messageId = packet.ToInt();

        ChatMessageBody body = QueuedMessages.Dequeue();
        body.SetMessageId(messageId);
    }
}