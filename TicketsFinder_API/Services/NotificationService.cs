﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TicketFinder_Models;
using TicketsFinder_API.Models;
using TicketsFinder_API.Models.Data;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public NotificationService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<int> CreateNotification(NotificationDTO notificationDTO)
        {
            int count = await _db.Notifications.CountAsync(n => n.ChatId == notificationDTO.ChatId);
            if (count == 3)
                return -1;

            await _db.Notifications.AddAsync(_mapper.Map<NotificationDTO, Notification>(notificationDTO));
            return await _db.SaveChangesAsync();
        }

        public async Task<int> DeleteNotification(Guid notificationId)
        {
            Notification? notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notification != null)
            {
                _db.Notifications.Remove(notification);
                return await _db.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<IEnumerable<NotificationDTO>> GetNotifications(int chatId)
        {
            return _mapper.Map<IEnumerable<Notification>, IEnumerable<NotificationDTO>>(_db.Notifications);
        }

        public async Task<int> UpdateNotification(NotificationDTO notificationDTO)
        {
            Notification? notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationDTO.Id);
            if (notification != null)
            {
                notification.From = notificationDTO.From;
                notification.To = notificationDTO.To;
                notification.Days = notificationDTO.Days;
                notification.Time = notificationDTO.Time;
                _db.Notifications.Update(notification);
                return await _db.SaveChangesAsync();
            }
            return 0;
        }
    }
}
