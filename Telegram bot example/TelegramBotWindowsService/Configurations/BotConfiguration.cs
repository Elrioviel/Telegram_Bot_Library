using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotWindowsService.Configurations
{
    public class BotConfiguration
    {
        public string BotToken { get; set; } = string.Empty;
        public long ChatId { get; set; }
    }
}
