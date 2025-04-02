using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram_bot__Library_.Interfaces
{
    internal interface ICommandHandler
    {
        Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}