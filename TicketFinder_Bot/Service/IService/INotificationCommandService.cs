using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TicketFinder_Bot.Service.IService
{
    public interface INotificationCommandService
    {
        public Task<int> CreateNotificationCommand(ITelegramBotClient botClient, Message message, int step, CancellationToken cancellationToken);
        public Task<int> UpdateNotificationCommand(ITelegramBotClient botClient, Message message, int step, CancellationToken cancellationToken);
        public Task<int> DeleteNotificationCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
        public Task GetNotificationsCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
        public Task<int> UpdateNotificationCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken);
        public Task<int> DeleteNotificationCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken);
    }
}
