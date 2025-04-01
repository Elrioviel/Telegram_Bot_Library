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

        public MessageHandler(ITelegramBotClient botClient, ILoggerService logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleMessageAsync(Message message)
        {
            if (message?.Text == null)
                return;

            var chatId = message.Chat.Id;
            _logger.Debug($"Received message: {message.Text} from {chatId}");

            if (message.ReplyToMessage != null)
            {
                await HandleReplyAsync(chatId, message.ReplyToMessage, message.Text);
            }
            else
            {
                await HandleCommandAsync(chatId, message.Text);
            }
        }

        private async Task HandleCommandAsync(long chatId, string messageText)
        {
            if (messageText.StartsWith("/start"))
            {
                await _botClient.SendMessage(chatId, "Welcome! I'm your bot.", parseMode: ParseMode.Html);

                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[] {new KeyboardButton("📋 Options"), new KeyboardButton("❓ Help") },
                    new[] {new KeyboardButton("❌ Cancel") }
                })
                {
                    ResizeKeyboard = true
                };

                await _botClient.SendMessage(chatId, "Choose and option:", replyMarkup: keyboard, parseMode: ParseMode.Html);
            }
            else if (messageText == "📋 Options")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("Click me!", "button_clicked") },
                    new[] { InlineKeyboardButton.WithUrl("Visit Google", "https://google.com") }
                });

                await _botClient.SendMessage(chatId, "Press a button:", replyMarkup: inlineKeyboard, parseMode: ParseMode.Html);
            }
            else
            {
                await _botClient.SendMessage(chatId, "Unknown command", parseMode: ParseMode.Html);
            }
        }

        private async Task HandleReplyAsync(long chatId, Message repliedMessage, string userReply)
        {
            _logger.Debug($"Bot was replied to: {repliedMessage.Text}, User response: {userReply}");

            if (repliedMessage.From?.IsBot == true)
            {
                await _botClient.SendMessage(chatId, $"You replied to my message with: {userReply}", parseMode: ParseMode.Html);
            }
        }
    }
}