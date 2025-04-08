using Moq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Services;
using Telegram.Bot.Types;
using Xunit;

namespace TelegramBotLibrary.Tests
{
    public class BotServiceTests
    {
        private readonly Mock<ITelegramBotClient> _botClientMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly BotService _botService;

        public BotServiceTests()
        {
            // Мокируем интерфейс ITelegramBotClient (для замены реального клиента)
            _botClientMock = new Mock<ITelegramBotClient>(MockBehavior.Loose);
            _loggerMock = new Mock<ILoggerService>();

            // Используем фиктивный токен для теста
            var testToken = "fake_test_token";

            // Создаем BotService с поддельным токеном
            _botService = new BotService(testToken, _loggerMock.Object);

            // Мокируем методы, которые вызываются в коде (GetMe, DeleteWebhook и DropPendingUpdates)
            _botClientMock.Setup(client => client.GetMe(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new User { Username = "test_bot" });
            _botClientMock.Setup(client => client.DeleteWebhook(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);
            _botClientMock.Setup(client => client.DropPendingUpdates(It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

            // Заменяем реальный клиент на замокированный с помощью рефлексии
            _botService.GetType()
                .GetProperty("BotClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_botService, _botClientMock.Object);
        }

        [Fact]
        public async Task StartAsync_ShouldStartBot_WhenCalled()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            await _botService.StartAsync(cancellationToken);

            // Assert
            _botClientMock.Verify(client => client.GetMe(It.IsAny<CancellationToken>()), Times.Once);
            _botClientMock.Verify(client => client.DeleteWebhook(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            _botClientMock.Verify(client => client.DropPendingUpdates(It.IsAny<CancellationToken>()), Times.Once);
            _loggerMock.Verify(logger => logger.Info(It.Is<string>(s => s.Contains("Bot started"))), Times.Once);
        }
    }
}




