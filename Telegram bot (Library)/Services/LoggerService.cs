using System.Collections.Concurrent;
using System.Text;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    /// <summary>
    /// Сервис для логирования сообщений бота в файл.
    /// </summary>
    internal sealed class LoggerService : ILoggerService
    {
        private static readonly string logDirectory = "Logs";
        private static readonly string logFilePath = Path.Combine(logDirectory, "bot_logs.txt");
        private static readonly ConcurrentQueue<string> logQueue = new();
        private static readonly SemaphoreSlim semaphore = new(1, 1);

        private enum LogLevel
        {
            Info,
            Debug,
            Error
        }

        public LoggerService()
        {
            // Создаём папку Logs, если её нет
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public void Info(string message) => Log(LogLevel.Info, message);
        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Error(string message) => Log(LogLevel.Error, message);

        /// <summary>
        /// Записывает лог-сообщение в очередь.
        /// </summary>
        private void Log(LogLevel level, string message)
        {
            string logMessage = $"{DateTime.Now} [{level}] {message}";
            logQueue.Enqueue(logMessage);

            // Запускаем в фоне, но при этом игнорируем возможные ошибки логирования.
            _ = Task.Run(WriteLogsToFileAsync).ConfigureAwait(false);
        }

        /// <summary>
        /// Асинхронно записывает накопленные логи в файл.
        /// </summary>
        private async Task WriteLogsToFileAsync()
        {
            if (!logQueue.IsEmpty)
            {
                await semaphore.WaitAsync();
                try
                {
                    StringBuilder sb = new();

                    while (logQueue.TryDequeue(out var log))
                    {
                        sb.AppendLine(log);
                    }

                    await File.AppendAllTextAsync(logFilePath, sb.ToString());
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }
    }
}