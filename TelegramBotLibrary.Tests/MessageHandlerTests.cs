using AutoFixture.AutoMoq;
using AutoFixture;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Services;

namespace TelegramBotLibrary.Tests
{
    public class MessageHandlerTests
    {
        // Моки для зависимостей
        private readonly Mock<ITelegramBotClient> _botMock;
        private readonly Mock<ILoggerService> _loggerMock;

        private readonly MessageHandler _messageHandler;

        // Общие данные для тестов
        private readonly long _chatId = 123L;
        private readonly CancellationToken _token = CancellationToken.None;

        public MessageHandlerTests()
        {
            // AutoFixture автоматически создает и настраивает моки
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            // "Замораживаем" моки, чтобы они переиспользовались
            _botMock = fixture.Freeze<Mock<ITelegramBotClient>>();
            _loggerMock = fixture.Freeze<Mock<ILoggerService>>();

            // Создаём экземпляр MessageHandler с подделанными зависимостями
            _messageHandler = new MessageHandler(_botMock.Object, _loggerMock.Object);

        }

        [Fact]
        public async Task HandleMessageAsync_ShouldCallOnMessageReceived()
        {
            // Arrage
            var message = new Message { Text = "Hello", Chat = new Chat { Id = _chatId } };
            var wasCalled = false;

            // Подписываемся на событие OnMessageReceived
            _messageHandler.OnMessageReceived += (_, _) =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            };

            // Act
            await _messageHandler.HandleMessageAsync(message, _token);

            // Assert
            Assert.True(wasCalled, "Expected OnMessageReceived handler to be called.");
        }

        [Fact]
        public async Task HandleMessageAsync_ShouldProcessKnownCommand()
        {
            // Arrange
            var command = "/start";
            var message = new Message { Text = command, Chat = new Chat { Id = _chatId } };

            // Флаг для проверки вызова обработчика команды
            var handlerCalled = false;
            
            _messageHandler.RegisterCommand(command, (_, _, _) =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await _messageHandler.HandleMessageAsync(message, _token);

            // Assert
            Assert.True(handlerCalled, $"Expected command handler for '{command}' to be called.");
        }

        [Fact]
        public async Task HandleMessageAsync_ShouldProcessReply()
        {
            // Arrange
            var promptText = "Введите email:";
            var replyText = "test@example.com";

            var botMessage = new Message { Text = promptText, From = new User { IsBot = true } };
            var userReply = new Message 
            { 
                Text = replyText, 
                Chat = new Chat { Id = _chatId }, 
                ReplyToMessage = botMessage 
            };

            // Флаг для проверки, был ли вызван обработчик ответа
            var wasHandlerCalled = false;

            _messageHandler.RegisterReply(promptText, (_, _, _, _) =>
            {
                wasHandlerCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await _messageHandler.HandleMessageAsync(userReply, _token);

            // Assert
            Assert.True(wasHandlerCalled, $"Expected reply handler for '{promptText}' to be called.");
        }
    }
}