using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram_bot__Library_.Interfaces
{
    /// <summary>
    /// Интерфейс для управления работой Telegram-бота.
    /// </summary>
    public interface IBotService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        Task SendMessageAsync(long chatId, string message);
        Task SendPhotoAsync(long chatId, string photoUrl, string caption = "");
        Task SendDocumentAsync(long chatId, string filePath, string customFileName = "", string caption = "");
        Task SendKeyboardAsync(long chatId, string message, IEnumerable<IEnumerable<string>> buttons);
        Task SendInlineKeyboardAsync(long chatId, string message, InlineKeyboardMarkup inlineKeyboard);
        IMessageHandler MessageHandler { get; }
    }
}