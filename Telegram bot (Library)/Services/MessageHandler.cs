using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    /// <summary>
    /// Обрабатывает входящие сообщения в Telegram боте.
    /// Поддерживает регистрацию команд и ответов.
    /// </summary>
    public sealed class MessageHandler : IMessageHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILoggerService _logger;

        // Словарь для хранения команд и их обработчиков.
        private readonly Dictionary<string, Func<Message, CancellationToken, Task>> _commands = new();

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
        public void RegisterCommand(string command, Func<Message, CancellationToken, Task> handler)
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
                await HandleCommandAsync(message.Chat.Id, message.Text, message.From?.FirstName, message.From?.LastName, message.From?.Username, cancellationToken);
            }
        }

        /// <summary>
        /// Обрабатывает команду от пользователя.
        /// </summary>
        public async Task HandleCommandAsync(long chatId, string messageText, string? firstName = null, string? lastName = null, string? username = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return;

            string command = messageText.Split(' ')[0].ToLowerInvariant();

            if (_commands.TryGetValue(command, out var handler))
            {
                try
                {
                    var message = new Message
                    {
                        Chat = new Chat { Id = chatId },
                        Text = messageText,
                        From = new User
                        {
                            Id = chatId, // Telegram user ID и chat ID часто совпадают в личных чатах
                            FirstName = firstName ?? "",
                            LastName = lastName,
                            Username = username
                        }
                    };

                    await handler.Invoke(message, cancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.Error($"Error executing command {command}: {exception}");
                    await _botClient.SendTextMessageAsync(chatId, "An error occurred while processing the command.", cancellationToken: cancellationToken);
                }
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId, "Unknown command.", cancellationToken: cancellationToken);
            }
        }


        /// <summary>
        /// Обрабатывает ответ на сообщение бота.
        /// </summary>
        public async Task HandleReplyAsync(long chatId, Message repliedMessage, string userReply, CancellationToken cancellationToken)
        {
            _logger.Debug($"Bot was replied to: {repliedMessage.Text}, User response: {userReply}");

            if (repliedMessage.From?.IsBot == true)
            {
                if (_replies.TryGetValue(repliedMessage.Text ?? "", out var handler))
                {
                    await handler.Invoke(chatId, repliedMessage, userReply, cancellationToken);
                }
                else if (userReply.StartsWith('/')) // В случае если пользователь ответил на сообщение бота с командой.
                {
                    // Попробуем достать инфу о пользователе
                    var fromUser = repliedMessage?.ReplyToMessage?.From;

                    await HandleCommandAsync(
                        chatId,
                        userReply,
                        fromUser?.FirstName,
                        fromUser?.LastName,
                        fromUser?.Username,
                        cancellationToken);
                }
                else
                {
                    await _botClient.SendTextMessageAsync(chatId, $"No handler registered for {userReply}", cancellationToken: cancellationToken);
                }
            }
        }
    }
}