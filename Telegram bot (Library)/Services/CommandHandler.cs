using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    internal sealed class CommandHandler : ICommandHandler
    {
        public event Func<Message, CancellationToken, Task> OnMessageReceived;
        public event Func<CallbackQuery, CancellationToken, Task> OnCallbackQueryReceived;

        private readonly ILoggerService _logger;

        public CommandHandler(ILoggerService logger)
        {
            _logger = logger;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message != null)
                {
                    if (OnMessageReceived != null)
                        await OnMessageReceived.Invoke(update.Message, cancellationToken);
                }
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                {
                    if (OnCallbackQueryReceived != null)
                        await OnCallbackQueryReceived.Invoke(update.CallbackQuery, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                _logger.Error($"Error: {exception.Message}");
            }
        }
    }
}