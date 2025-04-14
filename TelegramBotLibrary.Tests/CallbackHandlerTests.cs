using Moq;
using System.Security.Cryptography;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Services;

namespace TelegramBotLibrary.Tests
{
    public class CallbackHandlerTests
    {
        // Моки зависимостей
        private readonly Mock<ITelegramBotClient> _botClientMock;
        private readonly Mock<ILoggerService> _loggerMock;

        private readonly CallbackHandler _callbackHandler;
        private readonly CallbackQuery _callbackQuery;

        // Токен отмены
        private readonly CancellationToken _token = CancellationToken.None;

        public CallbackHandlerTests()
        {
            // Инициализация моков
            _botClientMock = new Mock<ITelegramBotClient>();
            _loggerMock = new Mock<ILoggerService>();

            // Создание объекта для тестирования с внедрёнными зависимостями
            _callbackHandler = new CallbackHandler(_botClientMock.Object, _loggerMock.Object);

            // Пример входящего callback-запроса
            _callbackQuery = new CallbackQuery
            {
                Id = "123",
                Data = "test"
            };
        }

        [Fact]
        public async Task HandleCallbackQueryAsync_WithoutHandler_CallsDefaultAnswer()
        {
            // Act
            await _callbackHandler.HandleCallbackQueryAsync(_callbackQuery, _token);

            // Assert
            _loggerMock.Verify(l => l.Debug(It.Is<string>(s => s.Contains("Callback received"))), Times.Once);
        }

        [Fact]
        public async Task HandleCallbackQueryAsync_WithHandler_InvokesHandler()
        {
            // Arrange
            bool handlerCalled = false;

            _callbackHandler.OnCallbackReceived += async (_, _) =>
            {
                handlerCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _callbackHandler.HandleCallbackQueryAsync(_callbackQuery, _token);

            // Assert
            Assert.True(handlerCalled);
        }

        [Fact]
        public async Task HandleCallbackQueryAsync_LogsDebug()
        {
            // Act
            await _callbackHandler.HandleCallbackQueryAsync(_callbackQuery, _token);

            // Assert
            _loggerMock.Verify( l => l.Debug(It.Is<string>(s => s.StartsWith("Callback received:"))), Times.Once);
        }

        [Fact]
        public async Task HandleCallbackQueryAsync_NoSubscribers_DoNotThrow()
        {
            // Act
            var exception = await Record.ExceptionAsync(() =>
            _callbackHandler.HandleCallbackQueryAsync(_callbackQuery, _token));

            // Assert
            Assert.Null(exception);
        }
    }
}