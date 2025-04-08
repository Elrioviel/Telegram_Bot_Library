using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Services;

namespace TelegramBotLibrary.Tests
{
    public class MessageHandlerTests
    {
        private readonly Mock<ITelegramBotClient> _botMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly MessageHandler _messageHandler;
        private readonly long _chatId = 123L ;

        public MessageHandlerTests()
        {
            _botMock = new Mock<ITelegramBotClient>();
            _loggerMock = new Mock<ILoggerService>();
            _messageHandler = new MessageHandler(_botMock.Object, _loggerMock.Object);

        }

        [Fact]
        public async Task HandleMessageAsync_ShouldCallOnMessageReceived()
        {
            // Arrage
            var message = new Message { Text = "Hello", Chat = new Chat { Id = _chatId } };
            var wasCalled = false;
            _messageHandler.OnMessageReceived += (msg, token) =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            };

            // Act
            await _messageHandler.HandleMessageAsync(message, CancellationToken.None);

            // Assert
            Assert.True(wasCalled);
        }

        [Fact]
        public async Task HandleMessageAsync_ShouldProcessKnownCommand()
        {
            // Arrange
            var commandText = "/start";
            var message = new Message { Text = commandText, Chat = new Chat { Id = _chatId } };

            var handlerCalled = false;
            _messageHandler.RegisterCommand(commandText, (id, text, token) =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await _messageHandler.HandleMessageAsync(message, CancellationToken.None);

            // Assert
            Assert.True(handlerCalled);
        }

        [Fact]
        public async Task HandleMessageAsync_ShouldProcessReply()
        {
            // Arrange
            var originalText = "Введите email:";
            var replyText = "test@example.com";

            var botMessage = new Message { Text = originalText, From = new User { IsBot = true } };
            var userReply = new Message { Text = replyText, Chat = new Chat { Id = _chatId }, ReplyToMessage = botMessage };

            var wasHandlerCalled = false;
            _messageHandler.RegisterReply(originalText, (id, msg, userInput, token) =>
            {
                wasHandlerCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await _messageHandler.HandleMessageAsync(userReply, CancellationToken.None);

            // Assert
            Assert.True(wasHandlerCalled);
        }
    }
}