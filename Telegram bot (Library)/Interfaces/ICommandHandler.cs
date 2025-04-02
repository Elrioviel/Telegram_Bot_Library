using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram_bot__Library_.Interfaces
{
    /// <summary>
    /// Интерфейс обработчика команд и обновлений.
    /// </summary>
    internal interface ICommandHandler
    {
        Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}