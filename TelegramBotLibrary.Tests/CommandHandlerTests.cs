using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Services;

namespace TelegramBotLibrary.Tests
{
    public class CommandHandlerTests
    {
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<ICallbackHandler> _callbackHandlerMock;
        private readonly Mock<IMessageHandler> _messageHandlerMock;
        private readonly Mock<ITelegramBotClient> _botClientMock;

        private readonly CommandHandler _commandHandler;

        public CommandHandlerTests()
        {
            _loggerMock = new Mock<ILoggerService>();
            _callbackHandlerMock = new Mock<ICallbackHandler>();
            _messageHandlerMock = new Mock<IMessageHandler>();
            _botClientMock = new Mock<ITelegramBotClient>();

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
            await _commandHandler.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            _messageHandlerMock.Verify(m => m.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task HandleUpdateAsync_CallbackQueryUpdate_CallsCallbackHandler()
        {
            // Arrange
            var callback = new CallbackQuery { Id = "123", Data = "click" };
            var update = new Update { CallbackQuery = callback };

            // Act
            await _commandHandler.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            _callbackHandlerMock.Verify(c => c.HandleCallbackQueryAsync(callback, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task HandleUpdateAsync_UnknownUpdate_DoesNothing()
        {
            // Arrange
            var update = new Update {};

            // Act
            await _commandHandler.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            _callbackHandlerMock.VerifyNoOtherCalls();
            _messageHandlerMock.VerifyNoOtherCalls();
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task HandleUpdateAsync_WhenExceptionThrown_LogsError()
        {
            // Arrange
            var message = new Message { Text = "test" };
            var update = new Update { Message = message };

            _messageHandlerMock
                .Setup(m => m.HandleMessageAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act
            await _commandHandler.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains("Test exception"))), Times.Once);
        }
    }
}
