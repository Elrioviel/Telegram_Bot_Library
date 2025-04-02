namespace Telegram_bot__Library_.Interfaces
{
    /// <summary>
    /// Интерфейс для логирования сообщений бота.
    /// </summary>
    internal interface ILoggerService
    {
        void Info(string message);
        void Debug(string message);
        void Error(string message);
    }
}