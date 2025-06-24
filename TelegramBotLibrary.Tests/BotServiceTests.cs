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
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

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

        [Fact]
        public async Task SendMessageAsync_ShouldStopBotWhenCalled()
        {
            // Act
            await _botService.StopAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(logger => logger.Info(It.Is<string>(s => s.Contains("Bot stopped"))), Times.Once);
        }

        [Fact]
        public async Task SendMessageAsync_ShouldSendMessage_WhenCalled()
        {
            // Arrange
            long chatId = 12345;
            string message = "Hello, World!";

            // Act
            await _botService.SendMessageAsync(chatId, message);

            // Assert
            _loggerMock.Verify(log => log.Info($"Sending message to chat {chatId}: {message}"), Times.Once);
        }

        [Fact]
        public async Task SendPhotoAsync_ShouldSendPhoto_WhenCalled()
        {
            // Arrange
            long chatId = 12345;
            string photoUrl = "photo.jpg";
            string caption = "Example photo";

            // Act
            await _botService.SendPhotoAsync(chatId, photoUrl, caption);

            // Assert
            _loggerMock.Verify(log => log.Info($"Sending photo to chat {chatId}"), Times.Once);
        }

        [Fact]
        public async Task SendDocumentAsync_ShouldSendDocument_WhenCalled()
        {
            // Arrange
            long chatId = 12345;
            string caption = "Example document";

            string tempFilePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFilePath, "Test content");

            try
            {
                // Act
                await _botService.SendDocumentAsync(chatId, tempFilePath, caption);

                // Assert
                _loggerMock.Verify(log => log.Info($"Sending document to chat {chatId}"), Times.Once);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task SendKeyboardAsync_ShouldSendKeyboard_WhenCalled()
        {
            // Arrange
            long chatId = 12345;
            string message = "Choose an option";
            var buttons = new[] { new[] { "Button 1", "Button 2" } };

            // Act
            await _botService.SendKeyboardAsync(chatId, message, buttons);

            // Assert
            _loggerMock.Verify(log => log.Info($"Sending keyboard reply to chat {chatId}"), Times.Once);
        }

        [Fact]
        public async Task SendInlineKeyboardAsync_ShouldSendInlineKeyboard_WhenCalled()
        {
            // Arrange
            long chatId = 12345;
            string message = "Choose and option";
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Button1", "Data1") }
            });

            // Act
            await _botService.SendInlineKeyboardAsync(chatId, message, inlineKeyboard);

            // Assert
            _loggerMock.Verify(log => log.Info($"Sending inline keyboard reply to chat {chatId}"), Times.Once);
        }

        [Fact]
        public async Task HandleErrorAsync_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            await _botService.HandleErrorAsync(_botClientMock.Object, exception, CancellationToken.None);

            // Assert
            _loggerMock.Verify(logger => logger.Error(It.Is<string>(s => s.Contains("Test exception"))), Times.Once);
        }
    }
}


