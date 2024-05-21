using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TicketFinder_Bot.Service.IService
{
    public interface ISearchCommandService
    {
        public Task<int> SearchCommand(ITelegramBotClient botClient, Message message, int step, CancellationToken cancellationToken);
        public Task<int> SearchCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken);
    }
}
