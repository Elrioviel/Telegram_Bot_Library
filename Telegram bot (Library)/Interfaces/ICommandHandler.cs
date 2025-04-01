using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram_bot__Library_.Interfaces
{
    internal interface ICommandHandler
    {
        event Func<Message, CancellationToken, Task> OnMessageReceived;
        event Func<CallbackQuery, CancellationToken, Task> OnCallbackQueryReceived;

        Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}