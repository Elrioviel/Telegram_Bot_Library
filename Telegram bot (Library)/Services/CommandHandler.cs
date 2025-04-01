using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram_bot__Library_.Services
{
    internal class CommandHandler
    {
        private readonly ITelegramBotClient _botClient;

        public CommandHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message?.Text != null)
                {
                    var messageText = update.Message.Text;
                    var chatId = update.Message.Chat.Id;

                    Logger.Debug($"Received message: {messageText} from {chatId}");

                    if (update.Message.ReplyToMessage != null)
                    {
                        await HandleReplyAsync(chatId, update.Message.ReplyToMessage, messageText);
                    }
                    else
                    {
                        await HandleCommandAsync(chatId, messageText);
                    }
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    await HandleCallbackQueryAsync(update.CallbackQuery);
                }
            }
            catch (Exception exception)
            {
                Logger.Error($"Error: {exception.Message}");
            }
        }

        private async Task HandleCommandAsync(long chatId, string messageText)
        {
            if (messageText.StartsWith("/start"))
            {
                await _botClient.SendTextMessageAsync(chatId, "Welcome! I'm your bot.");

                var keyBoard = new ReplyKeyboardMarkup(new[]
                {
                    new [] { new KeyboardButton("📋 Options"), new KeyboardButton ("❓ Help") },
                    new [] { new KeyboardButton("❌ Cancel") }
                })
                {
                    ResizeKeyboard = true
                };

                await _botClient.SendTextMessageAsync(chatId, "Choose an option:", replyMarkup: keyBoard);
            }
            else if (messageText == "📋 Options")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] { InlineKeyboardButton.WithCallbackData("Click me!", "button_clicked")},
                    new [] {InlineKeyboardButton.WithUrl("Visit Google", "https://google.com") }
                });

                await _botClient.SendTextMessageAsync(chatId, "Press a button:", replyMarkup: inlineKeyboard);
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId, "Unknown command.");
            }
        }

        private async Task HandleReplyAsync(long chatId, Message repliedMessage, string userReply)
        {
            Logger.Debug($"Bot was replied to: {repliedMessage.Text}, User response: {userReply}");

            if (repliedMessage.From?.IsBot == true)
            {
                await _botClient.SendTextMessageAsync(chatId, $"You replied to my message with: {userReply}");
            }
        }

        private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == "button_clicked")
            {
                await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "You clicked the button!");
                await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Button was clicked!");
            }
        }
    }
}
