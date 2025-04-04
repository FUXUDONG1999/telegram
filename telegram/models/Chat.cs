namespace Telegram.models;

public class Chat(
    int chatId,
    int messageId
)
{
    public int ChatId { get; } = chatId;

    public int MessageId { get; } = messageId;
}