using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TicketFinder_Bot.Service.IService
{
    public interface IHistoryCommandService
    {
        public Task HistoryCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
    }
}
