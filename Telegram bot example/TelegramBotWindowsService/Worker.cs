using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram_bot__Library_.Interfaces;
using TelegramBotWindowsService.Configurations;
using Telegram.Bot.Types.ReplyMarkups;

public class Worker : BackgroundService
{
    private readonly IBotService _botService;
    private readonly ILogger<Worker> _logger;
    private readonly long _chatId;

    public Worker(IBotService botService, ILogger<Worker> logger, IOptions<BotConfiguration> config)
    {
        _botService = botService;
        _logger = logger;
        _chatId = config.Value.ChatId;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        await _botService.StartAsync(stoppingToken);

        // Пример: Пауза, потом отправка фото
        await Task.Delay(5000);
        await _botService.SendMessageAsync(_chatId, "👋 Привет\\! Это тестовое сообщение из Windows Service\\.");

        var projectDirectory = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;

        var imagePath = Path.Combine(projectDirectory, "Resources", "telegram_logo.jpg");
        await _botService.SendPhotoAsync(_chatId, imagePath, "Локальное изображение");

        var documentPath = Path.Combine(projectDirectory, "Resources", "example.txt");
        await _botService.SendDocumentAsync(_chatId, documentPath, "📄 Пример документа");

        await _botService.SendKeyboardAsync(_chatId, "Выбери опцию:", new[]
        {
            new[] { "Кнопка 1", "Кнопка 2" },
            new[] { "Кнопка 3" }
        });

        var keyboard = new InlineKeyboardMarkup(
        new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔘 Кнопка", "action1")
            }
        });

        await _botService.SendInlineKeyboardAsync(_chatId, "Нажми кнопку:", keyboard);

        // Бесконечная работа
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10000, stoppingToken); // Просто "ждём", пока не остановится
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker stopping...");
        await _botService.StopAsync(cancellationToken);
    }
}
