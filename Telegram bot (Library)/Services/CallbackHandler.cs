using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram_bot__Library_.Services
{
    internal sealed class CallbackHandler
    {
        private readonly ITelegramBotClient _botClient;

        public CallbackHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == "button_clicked")
            {
                await _botClient.AnswerCallbackQuery(callbackQuery.Id, "You clicked the button!");
                await _botClient.SendMessage(callbackQuery.Message.Chat.Id, "Button was clicked!", parseMode: ParseMode.Html);
            }
        }
    }
}