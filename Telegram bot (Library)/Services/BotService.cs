using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Models;

namespace Telegram_bot__Library_.Services
{
    /// <summary>
    /// Основной сервис Telegram-бота отвечающий за его запуск, остановку и отправку сообщений.
    /// </summary>
    public sealed class BotService : IBotService, IHostedService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ICommandHandler _commandHandler;
        private readonly ILoggerService _logger;
        private readonly MessageHandler _messageHandler;
        private readonly CallbackHandler _callbackHandler;

        public IMessageHandler MessageHandler => _messageHandler;


        public BotService(IOptions<BotOptions> options, ILoggerService logger)
        {
            _logger = logger;

            var token = options?.Value?.Token?.Trim();

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Telegram Bot Token is null or empty.");

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.telegram.org/")
            };

            _botClient = new TelegramBotClient(token, httpClient);
            _messageHandler = new MessageHandler(_botClient, _logger);
            _callbackHandler = new CallbackHandler(_botClient, _logger);
            _commandHandler = new CommandHandler(_logger, _callbackHandler, _messageHandler);
        }

        /// <summary>
        /// Запускает бота и начинает обработку входящих сообщений.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции.</param>
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

        /// <summary>
        /// Останавливает бота.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.Info("Bot stopped");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Отправляет текстовое сообщение в указанный чат.
        /// </summary>
        /// <param name="chatId">ID чата.</param>
        /// <param name="message">Текст сообщения.</param>
        public async Task SendMessageAsync(long chatId, string message)
        {
            _logger.Info($"Sending message to chat {chatId}: {message}");

            await _botClient.SendMessage(chatId, message, parseMode: ParseMode.MarkdownV2);
        }

        /// <summary>
        /// Отправляет фото в указанный чат.
        /// </summary>
        /// <param name="chatId">ID чата.</param>
        /// <param name="photoUrl">URL изображения.</param>
        /// <param name="caption">Описание изображения.</param>
        public async Task SendPhotoAsync(long chatId, string photoUrl, string caption = "")
        {
            _logger.Info($"Sending photo to chat {chatId}");

            await using var stream = File.OpenRead(photoUrl);
            var inputFile = new InputFileStream(stream, Path.GetFileName(photoUrl));

            await _botClient.SendPhoto(chatId, inputFile, caption: caption);
        }

        /// <summary>
        /// Отправляет документ в указанный чат.
        /// </summary>
        /// <param name="chatId">ID чата.</param>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="caption">Описание файла.</param>
        public async Task SendDocumentAsync(long chatId, string filePath, string customFileName = "", string caption = "")
        {
            _logger.Info($"Sending document to chat {chatId}");

            var fileName = string.IsNullOrEmpty(customFileName) ? Path.GetFileName(filePath) : customFileName;

            await using var fileStream = File.OpenRead(filePath);
            await _botClient.SendDocument(chatId, new InputFileStream(fileStream, fileName), caption: caption);
        }

        /// <summary>
        /// Отправляет клавиатуру с кнопками в указанный чат.
        /// </summary>
        /// <param name="chatId">ID чата.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="buttons">Кнопки клавиатуры.</param>
        public async Task SendKeyboardAsync(long chatId, string message, IEnumerable<IEnumerable<string>> buttons)
        {
            _logger.Info($"Sending keyboard reply to chat {chatId}");

            var keyboard = new ReplyKeyboardMarkup(buttons.Select(row => row.Select(text => new KeyboardButton(text))))
            {
                ResizeKeyboard = true
            };

            await _botClient.SendMessage(chatId, message, replyMarkup: keyboard, parseMode: ParseMode.Html);
        }

        /// <summary>
        /// Отправляет inline-клавиатуру в указанный чат.
        /// </summary>
        /// <param name="chatId">ID чата.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="inlineKeyBoard">Inline-клавиатура.</param>
        public async Task SendInlineKeyboardAsync(long chatId, string message, InlineKeyboardMarkup inlineKeyBoard)
        {
            _logger.Info($"Sending inline keyboard reply to chat {chatId}");

            await _botClient.SendMessage(chatId, message, replyMarkup: inlineKeyBoard, parseMode: ParseMode.Html);
        }

        /// <summary>
        /// Обрабатывает ошибки, возникающие в работе бота.
        /// </summary>
        internal Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.Error($"Error: {exception}");
            return Task.CompletedTask;
        }
    }
}