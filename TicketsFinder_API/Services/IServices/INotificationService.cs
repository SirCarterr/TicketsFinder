using TicketFinder_Models;

namespace TicketsFinder_API.Services.IServices
{
    public interface INotificationService
    {
        public Task<int> CreateNotification(NotificationDTO notificationDTO);
        public Task<int> DeleteNotification(Guid notificationId);
        public Task<int> UpdateNotification(NotificationDTO notificationDTO);
        public Task<IEnumerable<NotificationDTO>> GetNotifications(int chatId);
    }
}
