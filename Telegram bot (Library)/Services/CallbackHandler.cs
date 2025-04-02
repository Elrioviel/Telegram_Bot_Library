using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram_bot__Library_.Services
{
    internal sealed class CallbackHandler
    {
        private readonly ITelegramBotClient _botClient;

        // Event that external apps can subscribe to.
        public event Func<CallbackQuery, CancellationToken, Task>? OnCallbackReceived;

        public CallbackHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (OnCallbackReceived != null)
            {
                // Invoke the external event handler if it's subscribed.
                await OnCallbackReceived.Invoke(callbackQuery, cancellationToken);
            }
            else
            {
                // Default behavior.
                await _botClient.AnswerCallbackQuery(callbackQuery.Id, "No handler registred.");
            }
        }
    }
}