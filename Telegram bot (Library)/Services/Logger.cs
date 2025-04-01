namespace Telegram_bot__Library_.Services
{
    internal class Logger
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

        public static void Info(string message) => Log(LogLevel.Info, message);
        public static void Debug(string message) => Log(LogLevel.Debug, message);
        public static void Error(string message) => Log(LogLevel.Error, message);
    }
}
