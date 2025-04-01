using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    internal sealed class BotService : IBotService, IHostedService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ICommandHandler _commandHandler;
        private readonly ILoggerService _logger;
        private readonly MessageHandler _messageHandler;
        private readonly CallbackHandler _callbackHandler;

        public BotService(string botToken, ILoggerService logger)
        {
            _logger = logger;
            _botClient = new TelegramBotClient(botToken);
            _commandHandler = new CommandHandler(_logger);
            _messageHandler = new MessageHandler(_botClient, _logger);
            _callbackHandler = new CallbackHandler(_botClient);

            // Подписаться на события.
            _commandHandler.OnMessageReceived += _messageHandler.HandleMessageAsync;
            _commandHandler.OnCallbackQueryReceived += _callbackHandler.HandleCallbackQueryAsync;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await _botClient.GetMe();
            await _botClient.DeleteWebhook();
            await _botClient.DropPendingUpdates();

            _logger.Info($"Bot started: {me.Username}");

            _botClient.StartReceiving(
                updateHandler: _commandHandler.HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: new ReceiverOptions { AllowedUpdates = { } },
                cancellationToken: cancellationToken
                );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.Info("Bot stopped");
            
            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(long chatId, string message)
        {
            await _botClient.SendMessage(chatId, message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        public async Task SendPhotoAsync(long chatId, string photoUrl, string caption = "")
        {
            await _botClient.SendPhoto(chatId, photoUrl, caption: caption);
        }

        public async Task SendDocumentAsync(long chatId, string filePath, string caption = "")
        {
            await using var fileStream = File.OpenRead(filePath);
            await _botClient.SendDocument(chatId, new InputFileStream(fileStream, filePath), caption: caption);
        }

        public async Task SendKeyboardAsync(long chatId, string message, IEnumerable<IEnumerable<string>> buttons)
        {
            var keyboard = new ReplyKeyboardMarkup(buttons.Select(row => row.Select(text => new KeyboardButton(text))))
            {
                ResizeKeyboard = true
            };

            await _botClient.SendMessage(chatId, message, replyMarkup: keyboard, parseMode: ParseMode.Html);
        }

        public async Task SendInlineKeyboardAsync(long chatId, string message, InlineKeyboardMarkup inlineKeyBoard)
        {
            await _botClient.SendMessage(chatId, message, replyMarkup: inlineKeyBoard, parseMode: ParseMode.Html);
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.Error($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}