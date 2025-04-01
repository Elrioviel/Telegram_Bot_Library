using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    internal class BotService : IBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly CommandHandler _commandHandler;

        public BotService (string botToken)
        {
            if (string.IsNullOrEmpty (botToken))
                throw new ArgumentException("Bot token cannot be null or empty", nameof (botToken));

            _botClient = new TelegramBotClient (botToken);
            _commandHandler = new CommandHandler(_botClient);
        }

        public async Task StartAsync()
        {
            var me = await _botClient.GetMeAsync();
            Logger.Info($"Bot started: {me.Username}");
            _botClient.StartReceiving(
            updateHandler: _commandHandler.HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: new ReceiverOptions { AllowedUpdates = { } },
            cancellationToken: CancellationToken.None
            );
        }

        public async Task StopAsync()
        {
            _botClient.CloseAsync();
            Logger.Info("Bot stopped");
        }

        public async Task SendMessageAsync(long chatId, string message)
        {
            await _botClient.SendTextMessageAsync(chatId, message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        public async Task SendPhotoAsync(long chatId, string photoUrl, string caption = "")
        {
            await _botClient.SendPhotoAsync(chatId, photoUrl, caption: caption);
        }

        public async Task SendDocumentAsync(long chatId, string filePath, string caption = "")
        {
            await using var fileStream = File.OpenRead(filePath);
            await _botClient.SendDocumentAsync(chatId, new InputFileStream(fileStream, filePath), caption: caption);
        }

        public async Task SendKeyboardAsync(long chatId, string message, IEnumerable<IEnumerable<string>> buttons)
        {
            var keyboard = new ReplyKeyboardMarkup(buttons.Select(row => row.Select(text => new KeyboardButton(text))))
            {
                ResizeKeyboard = true
            };

            await _botClient.SendTextMessageAsync(chatId, message, replyMarkup: keyboard);
        }

        public async Task SendInlineKeyboardAsync(long chatId, string message, InlineKeyboardMarkup inlineKeyBoard)
        {
            await _botClient.SendTextMessageAsync(chatId, message, replyMarkup: inlineKeyBoard);
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Logger.Error($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
