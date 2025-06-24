using Telegram.Bot.Types;

namespace Telegram_bot__Library_.Interfaces
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(Message message, CancellationToken cancellationToken);
        void RegisterCommand(string command, Func<Message, CancellationToken, Task> handler);
        void RegisterReply(string originalMessage, Func<long, Message, string, CancellationToken, Task> handler);
        Task HandleReplyAsync(long chatId, Message repliedMessage, string userReply, CancellationToken cancellationToken);
        Task HandleCommandAsync(long chatId, string messageText, string? firstName = null, string? lastName = null, string? username = null, CancellationToken cancellationToken = default);
    }
}