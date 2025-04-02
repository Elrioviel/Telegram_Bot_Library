using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    /// <summary>
    /// Обрабатывает входящие сообщения в Telegram боте.
    /// Поддерживает регистрацию команд и ответов.
    /// </summary>
    internal sealed class MessageHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILoggerService _logger;

        // Словарь для хранения команд и их обработчиков.
        private readonly Dictionary<string, Func<long, string, CancellationToken, Task>> _commands = new();

        // Словарь для хранения ответов и их обработчиков.
        private readonly Dictionary<string, Func<long, Message, string, CancellationToken, Task>> _replies = new();

        /// <summary>
        /// Событие которое вызывается при получении нового сообщения.
        /// </summary>
        public event Func<Message, CancellationToken, Task>? OnMessageReceived;

        public MessageHandler(ITelegramBotClient botClient, ILoggerService logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        /// <summary>
        /// Регистрирует команду и её обработчик.
        /// </summary>
        /// <param name="command">Название команды.</param>
        /// <param name="handler">Метод-обработчик команды.</param>
        public void RegisterCommand(string command, Func<long, string, CancellationToken, Task> handler)
        {
            _commands[command] = handler;
            _logger.Info($"Registred command: {command}");
        }

        /// <summary>
        /// Регистрирует ответ и его обработчик.
        /// </summary>
        /// <param name="originalMessage">Оригинальное сообщение на которое ожидается ответ.</param>
        /// <param name="handler">Метод-обработчик ответа.</param>
        public void RegisterReply(string originalMessage, Func<long, Message, string, CancellationToken, Task> handler)
        {
            _replies[originalMessage] = handler;
            _logger.Info($"Registred reply handler for: {originalMessage}");
        }

        /// <summary>
        /// Обрабатывает входящее сообщение от пользователя.
        /// </summary>
        public async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (message?.Text == null)
                return;

            _logger.Debug($"Received message: {message.Text} from {message.Chat.Id}");

            // Вызывам событие перед обработкой.

            // Перед вызовом сохранить обработчик в локальную переменную чтобы избежать NullReferenceException
            // если кто-то отпишется во время выполнения.
            var handler = OnMessageReceived;
            if (handler != null)
                await handler.Invoke(message, cancellationToken);

            if (message.ReplyToMessage != null)
            {
                await HandleReplyAsync(message.Chat.Id, message.ReplyToMessage, message.Text, cancellationToken);
            }
            else
            {
                await HandleCommandAsync(message.Chat.Id, message.Text, cancellationToken);
            }
        }

        /// <summary>
        /// Обрабатывает команду от пользователя.
        /// </summary>
        private async Task HandleCommandAsync(long chatId, string messageText, CancellationToken cancellationToken)
        {
            // Берем только саму команду, без аргументов.
            string command = messageText.Split(' ')[0];

            if (_commands.TryGetValue(command, out var handler))
            {
                try
                {
                    await handler.Invoke(chatId, messageText, cancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.Error($"Error executing command {command}: {exception}");
                    await _botClient.SendMessage(chatId, "An error occurred while processing the command.");
                }
            }
            else
            {
                await _botClient.SendMessage(chatId, "Unknown command.", parseMode: ParseMode.Html, cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Обрабатывает ответ на сообщение бота.
        /// </summary>
        private async Task HandleReplyAsync(long chatId, Message repliedMessage, string userReply, CancellationToken cancellationToken)
        {
            _logger.Debug($"Bot was replied to: {repliedMessage.Text}, User response: {userReply}");

            if (repliedMessage.From?.IsBot == true)
            {
                if (_replies.TryGetValue(repliedMessage.Text ?? "", out var handler))
                {
                    await handler.Invoke(chatId, repliedMessage, userReply, cancellationToken);
                }
                else if (userReply.StartsWith('/')) // В случае если пользователь ответит на сообщение бота с командой.
                {
                    await HandleCommandAsync(chatId, userReply, cancellationToken);
                }
                else
                {
                    await _botClient.SendMessage(chatId, $"No handler registred for {userReply}", parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                }
            }
        }
    }
}