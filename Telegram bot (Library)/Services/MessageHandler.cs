using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    internal sealed class MessageHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILoggerService _logger;

        // Словарь для хранения команд и их обработчиков.
        private readonly Dictionary<string, Func<long, string, CancellationToken, Task>> _commands = new();
        // Словарь для хранения ответов и их обработчиков.
        private readonly Dictionary<string, Func<long, Message, string, CancellationToken, Task>> _replies = new();

        public event Func<Message, CancellationToken, Task>? OnMessageReceived;

        public MessageHandler(ITelegramBotClient botClient, ILoggerService logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public void RegisterCommand(string command, Func<long, string, CancellationToken, Task> handler)
        {
            _commands[command] = handler;
            _logger.Info($"Registred command: {command}");
        }

        public void RegisterReply(string originalMessage, Func<long, Message, string, CancellationToken, Task> handler)
        {
            _replies[originalMessage] = handler;
            _logger.Info($"Registred reply handler for: {originalMessage}");
        }

        public async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (message?.Text == null)
                return;

            _logger.Debug($"Received message: {message.Text} from {message.Chat.Id}");

            // Вызывам событие перед обработкой.
            if (OnMessageReceived != null)
                await OnMessageReceived.Invoke(message, cancellationToken);

            if (message.ReplyToMessage != null)
            {
                await HandleReplyAsync(message.Chat.Id, message.ReplyToMessage, message.Text, cancellationToken);
            }
            else
            {
                await HandleCommandAsync(message.Chat.Id, message.Text, cancellationToken);
            }
        }

        private async Task HandleCommandAsync(long chatId, string messageText, CancellationToken cancellationToken)
        {
            // Берем только саму команду, без аргументов.
            string command = messageText.Split(' ')[0];

            if (_commands.TryGetValue(command, out var handler))
            {
                await handler.Invoke(chatId, messageText, cancellationToken);
            }
            else
            {
                await _botClient.SendMessage(chatId, "Unknown command.", parseMode: ParseMode.Html, cancellationToken: cancellationToken);
            }
        }

        private async Task HandleReplyAsync(long chatId, Message repliedMessage, string userReply, CancellationToken cancellationToken)
        {
            _logger.Debug($"Bot was replied to: {repliedMessage.Text}, User response: {userReply}");

            if (repliedMessage.From?.IsBot == true)
            {
                if (_replies.TryGetValue(repliedMessage.Text ?? "", out var handler))
                {
                    await handler.Invoke(chatId, repliedMessage, userReply, cancellationToken);
                }
                else
                {
                    await _botClient.SendMessage(chatId, $"No handler registred for {userReply}", parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                }
            }
        }
    }
}