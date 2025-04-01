using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram_bot__Library_.Interfaces
{
    internal interface ILoggerService
    {
        void Info(string message);
        void Debug(string message);
        void Error(string message);
    }
}
