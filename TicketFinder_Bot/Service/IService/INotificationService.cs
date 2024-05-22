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
        public Task<ResponseModelDTO> CreateNotification(NotificationDTO notificationDTO);
        public Task<ResponseModelDTO> UpdateNotification(NotificationDTO notificationDTO);
        public Task<ResponseModelDTO> DeleteNotification(NotificationDTO notificationDTO);
        public Task<ResponseModelDTO> GetNotifications(long? chatId);
    }
}
