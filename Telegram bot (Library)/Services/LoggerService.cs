using Telegram_bot__Library_.Interfaces;

namespace Telegram_bot__Library_.Services
{
    internal sealed class LoggerService : ILoggerService
    {
        private static readonly string logFilePath = "bot_logs.txt";
        private static readonly object lockObject = new();

        private enum LogLevel
        {
            Info,
            Debug,
            Error
        }

        private static void Log(LogLevel level, string message)
        {
            string logMessage = $"{DateTime.Now} [{level}] {message}";

            lock (lockObject)
            {
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
        }

        public void Info(string message) => Log(LogLevel.Info, message);
        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Error(string message) => Log(LogLevel.Error, message);
    }
}