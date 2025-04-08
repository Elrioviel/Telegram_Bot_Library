using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Services;

namespace TelegramBotLibrary.Tests
{
    public class CallbackHandlerTests
    {
        private readonly Mock<ITelegramBotClient> _botClientMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly CallbackHandler _callbackHandler;

        private readonly string _id = "123";
        private readonly string _data = "test";

        public CallbackHandlerTests()
        {
            _botClientMock = new Mock<ITelegramBotClient>();
            _loggerMock = new Mock<ILoggerService>();
            _callbackHandler = new CallbackHandler(_botClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task HandleCallbackQueryAsync_WithoutHandler_CallsDefaultAnswer()
        {
            // Arrange
            var callbackQuery = new CallbackQuery { Id = _id, Data = _data };

            // Act
            await _callbackHandler.HandleCallbackQueryAsync(callbackQuery, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.Debug(It.Is<string>(s => s.Contains("Callback received"))), Times.Once);
        }

        [Fact]
        public async Task HandleCallbackQueryAsync_WithHandler_InvokesHandler()
        {
            // Arrange
            var callbackQuery = new CallbackQuery { Id = _id, Data = _data };

            bool handlerCalled = false;

            _callbackHandler.OnCallbackReceived += async (query, token) =>
            {
                handlerCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _callbackHandler.HandleCallbackQueryAsync(callbackQuery, CancellationToken.None);

            // Assert
            Assert.True(handlerCalled);
        }

        [Fact]
        public async Task HandleCallbackQueryAsync_LogsDebug()
        {
            // Arrange
            var callbackQuery = new CallbackQuery { Id = _id, Data = _data };

            // Act
            await _callbackHandler.HandleCallbackQueryAsync(callbackQuery, CancellationToken.None);

            // Assert
            _loggerMock.Verify( l => l.Debug(It.Is<string>(s => s.StartsWith("Callback received:"))), Times.Once);
        }
    }
}
