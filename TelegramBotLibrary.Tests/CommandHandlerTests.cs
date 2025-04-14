using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Services;

namespace TelegramBotLibrary.Tests
{
    public class CommandHandlerTests
    {
        private readonly CommandHandler _commandHandler;

        // Моки (заглушки) для зависимостей
        private readonly Mock<ILoggerService> _loggerMock = new();
        private readonly Mock<ICallbackHandler> _callbackHandlerMock = new();
        private readonly Mock<IMessageHandler> _messageHandlerMock = new();
        private readonly Mock<ITelegramBotClient> _botClientMock = new();

        // Токен отмены
        private readonly CancellationToken _token = CancellationToken.None;

        public CommandHandlerTests()
        {
            // AutoFixture с AutoMoq позволяет автоматически создавать моки
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            // "Замораживаем" зависимости, чтобы они использовались единообразно везде
            _loggerMock = fixture.Freeze<Mock<ILoggerService>>();
            _callbackHandlerMock = fixture.Freeze<Mock<ICallbackHandler>>();
            _messageHandlerMock = fixture.Freeze<Mock<IMessageHandler>>();
            _botClientMock = fixture.Freeze<Mock<ITelegramBotClient>>();

            // Создаём экземпляр CommandHandler с подставленными зависимостями
            _commandHandler = new CommandHandler(
                _loggerMock.Object,
                _callbackHandlerMock.Object,
                _messageHandlerMock.Object);
        }

        [Fact]
        public async Task HandleUpdateAsync_MessageUpdate_CallsMessageHandler()
        {
            // Arrange
            var message = new Message { Text = "Hello" };
            var update = new Update { Message = message };

            // Act
            await _commandHandler.HandleUpdateAsync(_botClientMock.Object, update, _token);

            // Assert
            _messageHandlerMock.Verify(m => 
                m.HandleMessageAsync(message, _token),
                Times.Once());
        }

        [Fact]
        public async Task HandleUpdateAsync_CallbackQueryUpdate_CallsCallbackHandler()
        {
            // Arrange
            var callback = new CallbackQuery { Id = "123", Data = "click" };
            var update = new Update { CallbackQuery = callback };

            // Act
            await _commandHandler.HandleUpdateAsync(_botClientMock.Object, update, _token);

            // Assert
            _callbackHandlerMock.Verify(c =>
                c.HandleCallbackQueryAsync(callback, _token),
                Times.Once());
        }

        [Fact]
        public async Task HandleUpdateAsync_UnknownUpdate_DoesNothing()
        {
            // Arrange
            var update = new Update {};

            // Act
            await _commandHandler.HandleUpdateAsync(_botClientMock.Object, update, _token);

            // Assert
            _callbackHandlerMock.VerifyNoOtherCalls();
            _messageHandlerMock.VerifyNoOtherCalls();
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task HandleUpdateAsync_WhenExceptionThrown_LogsError()
        {
            // Arrange
            var update = new Update { Message = new Message { Text = "test" } };

            _messageHandlerMock
                .Setup(m => m.HandleMessageAsync(It.IsAny<Message>(), _token))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act
            await _commandHandler.HandleUpdateAsync(_botClientMock.Object, update, _token);

            // Assert
            _loggerMock.Verify(l =>
                l.Error(It.Is<string>(s => s.Contains("Test exception"))),
                Times.Once);
        }
    }
}