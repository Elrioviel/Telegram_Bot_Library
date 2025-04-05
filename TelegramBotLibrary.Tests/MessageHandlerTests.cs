using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Services;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotLibrary.Tests
{
    public class MessageHandlerTests
    {
        private readonly Mock<ITelegramBotClient> _botMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly MessageHandler _handler;

        [Fact]
        public void Test1()
        {

        }
    }
}
