using System.Collections.Concurrent;
using System.Text;
using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    internal sealed class LoggerService : ILoggerService
    {
        private static readonly string logFilePath = "bot_logs.txt";
        private static readonly ConcurrentQueue<string> logQueue = new();
        private static readonly SemaphoreSlim semaphore = new(1, 1);

        private enum LogLevel
        {
            Info,
            Debug,
            Error
        }

        public void Info(string message) => Log(LogLevel.Info, message);
        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Error(string message) => Log(LogLevel.Error, message);

        private void Log(LogLevel level, string message)
        {
            string logMessage = $"{DateTime.Now} [{level}] {message}";
            logQueue.Enqueue(logMessage);

            Task.Run(async () => await WriteLogsToFileAsync());
        }

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