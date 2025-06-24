using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    /// <summary>
    /// Обрабатывает callback-запросы, отправленные пользователями через inline-кнопки.
    /// </summary>
    public sealed class CallbackHandler : ICallbackHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILoggerService _logger;

        /// <summary>
        /// Событие, вызываемое при получении callback-запроса.
        /// </summary>
        public event Func<CallbackQuery, CancellationToken, Task>? OnCallbackReceived;

        public CallbackHandler(ITelegramBotClient botClient, ILoggerService logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        /// <summary>
        /// Обрабатывает callback-запрос.
        /// </summary>
        /// <param name="callbackQuery">Объект callback-запроса.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            _logger.Debug($"Callback received: {callbackQuery.Data}");

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