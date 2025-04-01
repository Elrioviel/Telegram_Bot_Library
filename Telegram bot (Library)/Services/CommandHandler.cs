using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    internal sealed class CommandHandler : ICommandHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly CallbackHandler _callbackHandler;
        private readonly ILoggerService _logger;

        public CommandHandler(ITelegramBotClient botClient, ILoggerService logger)
        {
            _logger = logger;
            _messageHandler = new MessageHandler(botClient, _logger);
            _callbackHandler = new CallbackHandler(botClient);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message)
                {
                    await _messageHandler.HandleMessageAsync(update.Message);
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    await _callbackHandler.HandleCallbackQueryAsync(update.CallbackQuery);
                }
            }
            catch (Exception exception)
            {
                _logger.Error($"Error: {exception.Message}");
            }
        }
    }
}
