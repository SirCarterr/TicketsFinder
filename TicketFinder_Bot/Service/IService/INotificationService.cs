using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketFinder_Models;

namespace TicketFinder_Bot.Service.IService
{
    public interface INotificationService
    {
        public NotificationDTO RequestNotificationDTO { get; set; }

        public Task<ResponseModelDTO> CreateNotification();
        public Task<ResponseModelDTO> UpdateNotification();
        public Task<ResponseModelDTO> DeleteNotification(Guid id);
        public Task<ResponseModelDTO> GetNotifications(long chatId);
    }
}
