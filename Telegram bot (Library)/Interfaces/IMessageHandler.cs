using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram_bot__Library_.Interfaces
{
    internal interface IMessageHandler
    {
        Task HandleMessageAsync(Message message, CancellationToken cancellationToken);
    }
}
