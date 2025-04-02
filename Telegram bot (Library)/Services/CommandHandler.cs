using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    /// <summary>
    /// Обработчик входящих сообщений и callback-запросов.
    /// </summary>
    internal sealed class CommandHandler : ICommandHandler
    {
        private readonly ILoggerService _logger;
        private readonly CallbackHandler _callbackHandler;
        private readonly MessageHandler _messageHandler;

        public CommandHandler(ILoggerService logger, CallbackHandler callbackHandler, MessageHandler messageHandler)
        {
            _logger = logger;
            _callbackHandler = callbackHandler;
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// Обрабатывает входящее обновление (сообщение или callback-запрос)
        /// </summary>
        /// <param name="botClient">Экземпляр Telegram-бота.</param>
        /// <param name="update">Объект обновления.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message != null)
                {
                    await _messageHandler.HandleMessageAsync(update.Message, cancellationToken);
                }
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                {
                    await _callbackHandler.HandleCallbackQueryAsync(update.CallbackQuery, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                _logger.Error($"Error: {exception}"); // Сохранить весь Exception чтобы не терять StackTrace.
            }
        }
    }
}