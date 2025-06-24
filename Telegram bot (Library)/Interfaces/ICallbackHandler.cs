using Telegram.Bot.Types;

namespace Telegram_bot__Library_.Interfaces
{
    public interface ICallbackHandler
    {
        Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken);
    }
}