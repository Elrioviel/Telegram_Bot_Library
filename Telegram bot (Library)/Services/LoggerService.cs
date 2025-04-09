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
        private static readonly string _logDirectory = "Logs";
        private static readonly string _logFilePath = Path.Combine(_logDirectory, "bot_logs.txt");
        private static readonly ConcurrentQueue<string> _logQueue = new();
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        private enum LogLevel
        {
            Info,
            Debug,
            Error
        }

        public LoggerService()
        {
            // Создаём папку Logs, если её нет
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
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
            _logQueue.Enqueue(logMessage);

            // Запускаем в фоне, но при этом игнорируем возможные ошибки логирования.
            _ = Task.Run(WriteLogsToFileAsync).ConfigureAwait(false);
        }

        /// <summary>
        /// Асинхронно записывает накопленные логи в файл.
        /// </summary>
        private async Task WriteLogsToFileAsync()
        {
            if (!_logQueue.IsEmpty)
            {
                await _semaphore.WaitAsync();
                try
                {
                    StringBuilder sb = new();

                    while (_logQueue.TryDequeue(out var log))
                    {
                        sb.AppendLine(log);
                    }

                    await File.AppendAllTextAsync(_logFilePath, sb.ToString());
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }
    }
}