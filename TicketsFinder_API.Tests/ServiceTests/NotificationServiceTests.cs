using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using TicketFinder_Models;
using TicketsFinder_API.Models;
using TicketsFinder_API.Models.Data;
using TicketsFinder_API.Services;

namespace TicketsFinder_API.Tests.ServiceTests
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private AppDbContext _dbContext;
        private Mock<IMapper> _mockMapper;
        private NotificationService _notificationService;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _mockMapper = new Mock<IMapper>();
            _notificationService = new NotificationService(_dbContext, _mockMapper.Object);

            _dbContext.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task CheckCount_ShouldReturn0_WhenCountIs3()
        {
            // Arrange 
            var chatId = 123L;
            _dbContext.Notifications.AddRange(new List<Notification>
            {
                new Notification { ChatId = chatId, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 },
                new Notification { ChatId = chatId, From = "From2", To = "To2", TicketTime = "00:00", Days = "day2", Time = "00:00", DaysToTrip = 1 },
                new Notification { ChatId = chatId, From = "From3", To = "To3", TicketTime = "00:00", Days = "day3", Time = "00:00", DaysToTrip = 1 }
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _notificationService.CheckCount(chatId);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task CheckCount_ShouldReturn1_WhenCountIsNot3()
        {
            // Arrange
            var chatId = 123L;
            _dbContext.Notifications.AddRange(new List<Notification>
            {
                new Notification { ChatId = chatId, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 },
                new Notification { ChatId = chatId, From = "From2", To = "To2", TicketTime = "00:00", Days = "day2", Time = "00:00", DaysToTrip = 1 },
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _notificationService.CheckCount(chatId);

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task CreateNotification_ShouldReturn1_WhenNotificationIsCreated()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { ChatId = 123L, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 };
            var notification = new Notification() { ChatId = 123, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 };
            _mockMapper.Setup(m => m.Map<NotificationDTO, Notification>(notificationDTO)).Returns(notification);

            // Act
            var result = await _notificationService.CreateNotification(notificationDTO);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            Assert.That(await _dbContext.Notifications.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteNotification_ShouldReturn1_WhenNotificationIsDeleted()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            var notification = new Notification { Id = notificationId, ChatId = 123L, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 };
            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _notificationService.DeleteNotification(notificationId);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            Assert.That(await _dbContext.Notifications.CountAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteNotification_ShouldReturn0_WhenNotificationIsNotFound()
        {
            // Arrange
            var notificationId = Guid.NewGuid();

            // Act
            var result = await _notificationService.DeleteNotification(notificationId);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetNotifications_ShouldReturnNotifications_ForSpecificChatId()
        {
            // Arrange
            var chatId = 123L;
            _dbContext.Notifications.AddRange(new List<Notification>
            {
                new Notification { ChatId = chatId, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 },
                new Notification { ChatId = chatId, From = "From2", To = "To2", TicketTime = "00:00", Days = "day2", Time = "00:00", DaysToTrip = 1 },
            });
            await _dbContext.SaveChangesAsync();

            var notificationDTOs = new List<NotificationDTO>
            {
                new NotificationDTO { ChatId = chatId, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 },
                new NotificationDTO { ChatId = chatId, From = "From2", To = "To2", TicketTime = "00:00", Days = "day2", Time = "00:00", DaysToTrip = 1 },
            };
            _mockMapper.Setup(m => m.Map<IEnumerable<Notification>, IEnumerable<NotificationDTO>>(It.IsAny<IEnumerable<Notification>>())).Returns(notificationDTOs);

            // Act
            var result = await _notificationService.GetNotifications(chatId);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetNotifications_ShouldReturnAllNotifications_WhenChatIdIsNull()
        {
            // Arrange
            _dbContext.Notifications.AddRange(new List<Notification>
            {
                new Notification { ChatId = 123L, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 },
                new Notification { ChatId = 456L, From = "From2", To = "To2", TicketTime = "00:00", Days = "day2", Time = "00:00", DaysToTrip = 1 },
            });
            await _dbContext.SaveChangesAsync();

            var notificationDTOs = new List<NotificationDTO>
            {
                new NotificationDTO { ChatId = 123L, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 },
                new NotificationDTO { ChatId = 456L, From = "From2", To = "To2", TicketTime = "00:00", Days = "day2", Time = "00:00", DaysToTrip = 1 },
            };
            _mockMapper.Setup(m => m.Map<IEnumerable<Notification>, IEnumerable<NotificationDTO>>(It.IsAny<IEnumerable<Notification>>())).Returns(notificationDTOs);

            // Act
            var result = await _notificationService.GetNotifications(null);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task UpdateNotification_ShouldReturn1_WhenNotificationIsUpdated()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { Id = Guid.NewGuid(), From = "From2", To = "To2", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 };
            var notification = new Notification { Id = notificationDTO.Id, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 };
            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _notificationService.UpdateNotification(notificationDTO);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            var updatedNotification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationDTO.Id);
            Assert.That(updatedNotification?.From, Is.EqualTo("From2"));
            Assert.That(updatedNotification.To, Is.EqualTo("To2"));
        }

        [Test]
        public async Task UpdateNotification_ShouldReturn0_WhenNotificationIsNotFound()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { Id = Guid.NewGuid(), From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 };

            // Act
            var result = await _notificationService.UpdateNotification(notificationDTO);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }
    }
}