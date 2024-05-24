using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Bot.Service;
using Telegram.Bot.Types;
using TicketFinder_Common;
using TicketFinder_Models;

namespace TicketsFinder_Bot.Tests
{
    [TestFixture]
    public class NotificationCommandServiceTests
    {
        private Mock<INotificationService> _notificationService;
        private Mock<IValidationService> _validationServiceMock;
        private NotificationCommandService _notificationCommandService;
        private Mock<ITelegramBotClient> _botClientMock;

        [SetUp]
        public void Setup()
        {
            _notificationService = new Mock<INotificationService>();
            _validationServiceMock = new Mock<IValidationService>();
            _notificationCommandService = new NotificationCommandService(_notificationService.Object, _validationServiceMock.Object);
            _botClientMock = new Mock<ITelegramBotClient>();
        }

        [Test]
        public async Task CreateNotificationCommand_ShouldReturnNextStep_WhenStepIsZero()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "test" };
            var cancellationToken = new CancellationToken();
            int step = 0;
            _notificationService.Setup(x => x.GetNotifications(It.IsAny<long>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = true,
                Data = new List<NotificationDTO>()
                {
                    new(), new()
                }
            });

            // Act
            var result = await _notificationCommandService.CreateNotificationCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task CreateNotificationCommand_ShouldReturnLimitExceeded_WhenThreeNotificationsIsRetireved()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "test" };
            var cancellationToken = new CancellationToken();
            int step = 0;
            _notificationService.Setup(x => x.GetNotifications(It.IsAny<long>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = true,
                Data = new List<NotificationDTO>()
                {
                    new(), new(), new()
                }
            });

            // Act
            var result = await _notificationCommandService.CreateNotificationCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task CreateNotificationCommand_ShouldReturnCannotCreateNotification_WhenResponseIsFailed()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "test" };
            var cancellationToken = new CancellationToken();
            int step = 0;
            _notificationService.Setup(x => x.GetNotifications(It.IsAny<long>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = false,
            });

            // Act
            var result = await _notificationCommandService.CreateNotificationCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task CreateNotificationCommand_ShouldReturnSameStep_WhenValidationFails()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "invalid input" };
            var cancellationToken = new CancellationToken();
            int step = 1;
            _validationServiceMock.Setup(x => x.ValidateRoute(It.IsAny<string>())).Returns(new[] { null, "Error message" });

            // Act
            var result = await _notificationCommandService.CreateNotificationCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(step, result);
        }

        [Test]
        public async Task CreateNotificationCommand_ShouldReturnNextStep_WhenValidationSucceeds()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "valid input" };
            var cancellationToken = new CancellationToken();
            int step = 1;
            _validationServiceMock.Setup(x => x.ValidateRoute(It.IsAny<string>())).Returns(new[] { "from", "to" });

            // Act
            var result = await _notificationCommandService.CreateNotificationCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(2, result);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task CreateNotificationCommand_ShouldReturnMessage_WhenFinalStepIsReached(bool responseResult)
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "valid input" };
            var cancellationToken = new CancellationToken();
            int step = SD.notificationCreate_command_steps;
            _validationServiceMock.Setup(x => x.ValidateDaysNumber(It.IsAny<string>())).Returns(new[] { "2", null });
            _notificationService.Setup(x => x.CreateNotification(It.IsAny<NotificationDTO>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = responseResult
            });

            // Act
            var result = await _notificationCommandService.CreateNotificationCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task DeleteNotificationCallback_ShouldReturnCorrectResponse_WhenCalledWithValidData()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var callbackQuery = new CallbackQuery
            {
                Data = "notificationDelete " + id,
                Message = new Message { Chat = new Chat { Id = 1 }, MessageId = 1 }
            };
            var cancellationToken = new CancellationToken();
            _notificationService.Setup(x => x.GetNotifications(It.IsAny<long>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = true,
                Data = new List<NotificationDTO>()
                {
                    new() { Id = id, ChatId = 1 }
                }
            });

            // Act
            var result = await _notificationCommandService.DeleteNotificationCallback(_botClientMock.Object, callbackQuery, cancellationToken);

            // Assert
            Assert.AreEqual(1, result);
            _notificationService.Verify(x => x.GetNotifications(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task DeleteNotificationCallback_ShouldReturnInorrectResponse_WhenNotificationIsNotFound()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var callbackQuery = new CallbackQuery
            {
                Data = "notificationDelete " + id,
                Message = new Message { Chat = new Chat { Id = 1 }, MessageId = 1 }
            };
            var cancellationToken = new CancellationToken();
            _notificationService.Setup(x => x.GetNotifications(It.IsAny<long>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = true,
                Data = new List<NotificationDTO>()
                {
                    new() { Id = Guid.Empty, ChatId = 1 }
                }
            });

            // Act
            var result = await _notificationCommandService.DeleteNotificationCallback(_botClientMock.Object, callbackQuery, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
            _notificationService.Verify(x => x.GetNotifications(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task DeleteNotificationCallback_ShouldReturnInorrectResponse_WhenRequestIsFailed()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var callbackQuery = new CallbackQuery
            {
                Data = "notificationDelete " + id,
                Message = new Message { Chat = new Chat { Id = 1 }, MessageId = 1 }
            };
            var cancellationToken = new CancellationToken();
            _notificationService.Setup(x => x.GetNotifications(It.IsAny<long>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = false,
            });

            // Act
            var result = await _notificationCommandService.DeleteNotificationCallback(_botClientMock.Object, callbackQuery, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
            _notificationService.Verify(x => x.GetNotifications(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task DeleteNotificationCommand_ShouldReturnDeletedMessage_WhenUserConfirm()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "так" };
            var cancellationToken = new CancellationToken();
            _notificationService.Setup(x => x.DeleteNotification(It.IsAny<NotificationDTO>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = true,
            });

            // Act
            var result = await _notificationCommandService.DeleteNotificationCommand(_botClientMock.Object, message, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task DeleteNotificationCommand_ShouldReturnCancelMessage_WhenUserDecline()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "ні" };
            var cancellationToken = new CancellationToken();
            _notificationService.Setup(x => x.DeleteNotification(It.IsAny<NotificationDTO>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = true,
            });

            // Act
            var result = await _notificationCommandService.DeleteNotificationCommand(_botClientMock.Object, message, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task DeleteNotificationCommand_ShouldReturnErrorMessage_WhenRequestIsFailed()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "ні" };
            var cancellationToken = new CancellationToken();
            _notificationService.Setup(x => x.DeleteNotification(It.IsAny<NotificationDTO>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = false,
            });

            // Act
            var result = await _notificationCommandService.DeleteNotificationCommand(_botClientMock.Object, message, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task UpdateNotificationCallback_ShouldReturnCorrectResponse_WhenCalledWithValidData()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var callbackQuery = new CallbackQuery
            {
                Data = "notificationUpdate " + id,
                Message = new Message { Chat = new Chat { Id = 1 }, MessageId = 1 }
            };
            var cancellationToken = new CancellationToken();
            _notificationService.Setup(x => x.GetNotifications(It.IsAny<long>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = true,
                Data = new List<NotificationDTO>()
                {
                    new() { Id = id, ChatId = 1 }
                }
            });

            // Act
            var result = await _notificationCommandService.UpdateNotificationCallback(_botClientMock.Object, callbackQuery, cancellationToken);

            // Assert
            Assert.AreEqual(1, result);
            _notificationService.Verify(x => x.GetNotifications(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task UpdateNotificationCallback_ShouldReturnInorrectResponse_WhenNotificationIsNotFound()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var callbackQuery = new CallbackQuery
            {
                Data = "notificationUpdate " + id,
                Message = new Message { Chat = new Chat { Id = 1 }, MessageId = 1 }
            };
            var cancellationToken = new CancellationToken();
            _notificationService.Setup(x => x.GetNotifications(It.IsAny<long>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = true,
                Data = new List<NotificationDTO>()
                {
                    new() { Id = Guid.NewGuid(), ChatId = 1 }
                }
            });

            // Act
            var result = await _notificationCommandService.UpdateNotificationCallback(_botClientMock.Object, callbackQuery, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
            _notificationService.Verify(x => x.GetNotifications(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task UpdateNotificationCallback_ShouldReturnInorrectResponse_WhenRequestIsFailed()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var callbackQuery = new CallbackQuery
            {
                Data = "notificationDelete " + id,
                Message = new Message { Chat = new Chat { Id = 1 }, MessageId = 1 }
            };
            var cancellationToken = new CancellationToken();
            _notificationService.Setup(x => x.GetNotifications(It.IsAny<long>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = false
            });

            // Act
            var result = await _notificationCommandService.UpdateNotificationCallback(_botClientMock.Object, callbackQuery, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
            _notificationService.Verify(x => x.GetNotifications(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task UpdateNotificationCommand_ShouldReturnNextStep_WhenFieldSkipped()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "-" };
            var cancellationToken = new CancellationToken();
            int step = 1;

            // Act
            var result = await _notificationCommandService.UpdateNotificationCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(2, result);
        }

        [Test]
        public async Task UpdateNotificationCommand_ShouldReturnNextStep_WhenValidationSucceeds()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "valid input" };
            var cancellationToken = new CancellationToken();
            int step = 1;
            _validationServiceMock.Setup(x => x.ValidateRoute(It.IsAny<string>())).Returns(new[] { "from", "to" });

            // Act
            var result = await _notificationCommandService.UpdateNotificationCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(2, result);
        }

        [Test]
        public async Task UpdateNotificationCommand_ShouldReturnSameStep_WhenValidationFails()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "invalid input" };
            var cancellationToken = new CancellationToken();
            int step = 1;
            _validationServiceMock.Setup(x => x.ValidateRoute(It.IsAny<string>())).Returns(new[] { null, "Error message" });

            // Act
            var result = await _notificationCommandService.CreateNotificationCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(step, result);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task UpdateNotificationCommand_ShouldReturnMessage_WhenFinalStepIsReached(bool responseResult)
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "valid input" };
            var cancellationToken = new CancellationToken();
            int step = SD.notificationCreate_command_steps;
            _validationServiceMock.Setup(x => x.ValidateDaysNumber(It.IsAny<string>())).Returns(new[] { "2", null });
            _notificationService.Setup(x => x.UpdateNotification(It.IsAny<NotificationDTO>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = responseResult,
            });

            // Act
            var result = await _notificationCommandService.UpdateNotificationCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
        }
    }
}
