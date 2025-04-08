using Telegram_bot__Library_.Services;

namespace TelegramBotLibrary.Tests
{
    public class LoggerServiceTests : IDisposable
    {
        private readonly string _logFilePath = Path.Combine("Logs", "bot_logs.txt");

        // Arrange
        private readonly string _infoMessage = "Info message.";
        private readonly string _debugMessage = "Debug message.";
        private readonly string _errorMessage = "Something went wrong.";
        private readonly string _info = "[Info]";
        private readonly string _debug = "[Debug]";
        private readonly string _error = "[Error]";

        private LoggerService _logger;

        public LoggerServiceTests()
        {
            if (File.Exists(_logFilePath))
                File.Delete(_logFilePath);

            _logger = new LoggerService();
        }

        [Fact]
        public async Task Info_ShouldLogMessageToFile()
        {
            // Act
            _logger.Info(_infoMessage);
            await Task.Delay(300);

            // Assert
            string content = File.ReadAllText(_logFilePath);
            Assert.Contains(_info, content);
            Assert.Contains(_infoMessage, content);
        }

        [Fact]
        public async Task Debug_ShouldLogMessageToFile()
        {
            // Act
            _logger.Debug(_debugMessage);
            await Task.Delay(300);

            // Assert
            string content = File.ReadAllText(_logFilePath);
            Assert.Contains(_debug, content);
            Assert.Contains(_debugMessage, content);
        }

        [Fact]
        public async Task Error_ShouldLogMessageToFile()
        {
            // Act
            _logger.Error(_errorMessage);
            await Task.Delay(300);

            // Assert
            string content = File.ReadAllText(_logFilePath);
            Assert.Contains(_error, content);
            Assert.Contains(_errorMessage, content);
        }

        [Fact]
        public async Task MultipleLogs_ShouldLogAllMessages()
        {
            // Act
            _logger.Info(_infoMessage);
            _logger.Debug(_debugMessage);
            _logger.Error(_errorMessage);

            await Task.Delay(600);

            // Assert
            string content = File.ReadAllText(_logFilePath);
            Assert.Contains(_info, content);
            Assert.Contains(_infoMessage, content);
            Assert.Contains(_debug, content);
            Assert.Contains(_debugMessage, content);
            Assert.Contains(_error, content);
            Assert.Contains(_errorMessage, content);
        }

        public void Dispose()
        {
            if (File.Exists(_logFilePath))
                File.Delete(_logFilePath);
        }
    }
}