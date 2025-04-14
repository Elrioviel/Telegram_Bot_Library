using Telegram_bot__Library_.Services;

namespace TelegramBotLibrary.Tests
{
    public class LoggerServiceTests : IDisposable
    {
        private readonly LoggerService _logger = new();
        private readonly string _logFilePath = Path.Combine("Logs", "bot_logs.txt");

        // Префиксы, по которым будем проверять записи в логах
        private readonly string _infoPrefix = "[Info]";
        private readonly string _debugPrefix = "[Debug]";
        private readonly string _errorPrefix = "[Error]";

        // Примерные сообщения для логирования
        private readonly string _infoMessage = "Info message.";
        private readonly string _debugMessage = "Debug message.";
        private readonly string _errorMessage = "Something went wrong.";

        public LoggerServiceTests()
        {
            if (File.Exists(_logFilePath))
                File.Delete(_logFilePath);
        }

        [Fact]
        public async Task Info_ShouldLogMessageToFile()
        {
            // Act
            _logger.Info(_infoMessage);
            await FlushAndWait(); // даем логгеру время записать данные

            // Assert
            string content = File.ReadAllText(_logFilePath);
            Assert.Contains(_infoPrefix, content);
            Assert.Contains(_infoMessage, content);
        }

        [Fact]
        public async Task Debug_ShouldLogMessageToFile()
        {
            // Act
            _logger.Debug(_debugMessage);
            await FlushAndWait();

            // Assert
            string content = File.ReadAllText(_logFilePath);
            Assert.Contains(_debugPrefix, content);
            Assert.Contains(_debugMessage, content);
        }

        [Fact]
        public async Task Error_ShouldLogMessageToFile()
        {
            // Act
            _logger.Error(_errorMessage);
            await FlushAndWait();

            // Assert
            string content = File.ReadAllText(_logFilePath);
            Assert.Contains(_errorPrefix, content);
            Assert.Contains(_errorMessage, content);
        }

        [Fact]
        public async Task MultipleLogs_ShouldLogAllMessages()
        {
            // Act
            _logger.Info(_infoMessage);
            _logger.Debug(_debugMessage);
            _logger.Error(_errorMessage);

            await FlushAndWait();

            // Assert
            string content = File.ReadAllText(_logFilePath);
            Assert.Contains(_infoPrefix, content);
            Assert.Contains(_infoMessage, content);
            Assert.Contains(_debugPrefix, content);
            Assert.Contains(_debugMessage, content);
            Assert.Contains(_errorPrefix, content);
            Assert.Contains(_errorMessage, content);
        }

        private static Task FlushAndWait() => Task.Delay(300);

        public void Dispose()
        {
            if (File.Exists(_logFilePath))
                File.Delete(_logFilePath);
        }
    }
}