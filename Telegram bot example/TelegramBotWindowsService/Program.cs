using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram_bot__Library_.Interfaces;
using Telegram_bot__Library_.Services;
using TelegramBotWindowsService.Configurations;
using TelegramBotWindowsService.Models;

var builder = Host.CreateApplicationBuilder(args);

// Загружаем конфигурацию из appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Привязываем секцию BotConfiguration
builder.Services.Configure<BotConfiguration>(
    builder.Configuration.GetSection("BotConfiguration"));

// Регистрируем зависимые сервисы
builder.Services.AddSingleton<ILoggerService, LoggerService>();

var tokenTest = builder.Configuration.GetSection("BotConfiguration")["BotToken"];

builder.Services.AddSingleton<IBotService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>().GetSection("BotConfiguration").Get<BotConfiguration>()!;

    var botOptions = new BotOptions { };
    botOptions.Token = config.BotToken;

    var logger = provider.GetRequiredService<ILoggerService>();
    return new BotService((Microsoft.Extensions.Options.IOptions<Telegram_bot__Library_.Models.BotOptions>)botOptions, logger);
});

// Регистрируем воркер
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
