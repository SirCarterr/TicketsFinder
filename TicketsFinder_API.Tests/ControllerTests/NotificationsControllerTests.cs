using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketFinder_Models;
using TicketsFinder_API.Controllers;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Tests.ControllerTests
{
    [TestFixture]
    public class NotificationsControllerTests
    {
        private Mock<INotificationService> _mockNotificationService;
        private Mock<ILogger<NotificationsController>> _mockLogger;
        private NotificationsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<NotificationsController>>();
            _controller = new NotificationsController(_mockNotificationService.Object, _mockLogger.Object);

        }

        [Test]
        public async Task Create_ShouldReturn201_WhenCheckCountIs1()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { ChatId = 123L, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 };
            _mockNotificationService.Setup(s => s.CheckCount(notificationDTO.ChatId)).ReturnsAsync(1);
            _mockNotificationService.Setup(s => s.CreateNotification(notificationDTO)).ReturnsAsync(1);

            // Act
            var result = await _controller.Create(notificationDTO);

            // Assert
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status201Created, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Create_ShouldReturn405_WhenCheckCountIsNot1()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { ChatId = 123L, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 };
            _mockNotificationService.Setup(s => s.CheckCount(notificationDTO.ChatId)).ReturnsAsync(0);

            // Act
            var result = await _controller.Create(notificationDTO);

            // Assert
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status405MethodNotAllowed, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Create_ShouldReturn500_WhenExceptionIsThrown()
        {
            // Arrange
            var notificationDTO = new NotificationDTO {ChatId = 123L, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 };
            _mockNotificationService.Setup(s => s.CheckCount(notificationDTO.ChatId)).ThrowsAsync(new Exception("Some error"));

            // Act
            var result = await _controller.Create(notificationDTO);

            // Assert
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Update_ShouldReturn202_WhenUpdateIsSuccessful()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { Id = Guid.NewGuid(), ChatId = 1 };
            _mockNotificationService.Setup(s => s.UpdateNotification(notificationDTO)).ReturnsAsync(1);

            // Act
            var result = await _controller.Update(notificationDTO);

            // Assert
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status202Accepted, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Update_ShouldReturn404_WhenNotificationIsNotFound()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { Id = Guid.NewGuid(), ChatId = 1 };
            _mockNotificationService.Setup(s => s.UpdateNotification(notificationDTO)).ReturnsAsync(0);

            // Act
            var result = await _controller.Update(notificationDTO);

            // Assert
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Update_ShouldReturn500_WhenExceptionIsThrown()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { Id = Guid.NewGuid(), ChatId = 1 };
            _mockNotificationService.Setup(s => s.UpdateNotification(notificationDTO)).ThrowsAsync(new Exception("Some error"));

            // Act
            var result = await _controller.Update(notificationDTO);

            // Assert
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Delete_ShouldReturn200_WhenDeletionIsSuccessful()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            _mockNotificationService.Setup(s => s.DeleteNotification(notificationId)).ReturnsAsync(1);

            // Act
            var result = await _controller.Delete(notificationId);

            // Assert
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Delete_ShouldReturn404_WhenNotificationIsNotFound()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            _mockNotificationService.Setup(s => s.DeleteNotification(notificationId)).ReturnsAsync(0);

            // Act
            var result = await _controller.Delete(notificationId);

            // Assert
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Delete_ShouldReturn500_WhenExceptionIsThrown()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            _mockNotificationService.Setup(s => s.DeleteNotification(notificationId)).ThrowsAsync(new Exception("Some error"));

            // Act
            var result = await _controller.Delete(notificationId);

            // Assert
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Get_ShouldReturn200_WhenNotificationsAreRetrieved()
        {
            // Arrange
            long chatId = 123L;
            var notifications = new List<NotificationDTO> { new() { ChatId = chatId, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 } };
            _mockNotificationService.Setup(s => s.GetNotifications(chatId)).ReturnsAsync(notifications);

            // Act
            var result = await _controller.Get(chatId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual(notifications, okResult.Value);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Get_ShouldReturn200_WhenNoChatIdIsProvided()
        {
            // Arrange
            var notifications = new List<NotificationDTO> { new() { ChatId = 123L, From = "From1", To = "To1", TicketTime = "00:00", Days = "day1", Time = "00:00", DaysToTrip = 1 } };
            _mockNotificationService.Setup(s => s.GetNotifications(null)).ReturnsAsync(notifications);

            // Act
            var result = await _controller.Get(null);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual(notifications, okResult.Value);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }
    }
}
