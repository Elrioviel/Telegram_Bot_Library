using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Services;
using Telegram.Bot.Types;
using Xunit;
using System.Reflection;
using Telegram.Bot.Types.Enums;

namespace TelegramBotLibrary.Tests
{
    public class BotServiceTests
    {
        private readonly Mock<ITelegramBotClient> _botClientMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly BotService _botService;

        public BotServiceTests()
        {
            _botClientMock = new Mock<ITelegramBotClient>(); // Строгий мок без дополнительных расширений
            _loggerMock = new Mock<ILoggerService>();

            // Мокаем именно SendRequest, потому что GetMe — extension method
            _botClientMock
                .Setup(client => client.SendRequest(
                    It.Is<GetMeRequest>(r => r != null),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Username = "test_bot" });

            _botClientMock
                .Setup(client => client.SendRequest(
                    It.Is<DeleteWebhookRequest>(r => r.DropPendingUpdates == true),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Теперь конструктор BotService принимает уже замокированный объект TelegramBotClient
            _botService = new BotService("123456789:FAKE_token_for_test", _loggerMock.Object);

            /// Устанавливаем мок в свойство или поле напрямую, если конструкция BotService позволяет это
            var botClientProperty = _botService.GetType()
                .GetField("_botClient", BindingFlags.NonPublic | BindingFlags.Instance); // Может быть поле с именем _botClient

            if (botClientProperty != null)
            {
                botClientProperty.SetValue(_botService, _botClientMock.Object);
            }
        }

        [Fact]
        public async Task StartAsync_ShouldStartBot_WhenCalled()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Настройка мока GetMe с возвращаемым объектом User
            _botClientMock
                .Setup(client => client.SendRequest(
                    It.Is<GetMeRequest>(r => r != null),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Username = "test_bot" });

            // Настройка мока для DeleteWebhook и DropPendingUpdates
            _botClientMock
                .Setup(client => client.SendRequest(
                    It.Is<DeleteWebhookRequest>(r => r.DropPendingUpdates == true),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _botClientMock
                .Setup(client => client.SendRequest<Update[]>(
                    It.Is<GetUpdatesRequest>(r => true),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Update>());

            // Act
            await _botService.StartAsync(CancellationToken.None);

            // Assert

            _loggerMock.Verify(logger => logger.Info(It.Is<string>(s => s.Contains("Bot started"))), Times.Once);
        }
    }
}


