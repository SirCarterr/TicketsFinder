using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TicketFinder_Bot.Service;
using TicketFinder_Models;

namespace TicketsFinder_Bot.Tests
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private NotificationService _notificationService;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            _notificationService = new NotificationService(_httpClient);
        }

        //Create
        [Test]
        public async Task CreateNotification_Successful_ReturnsSuccessMessage()
        {
            // Arrange
            var notificationDTO = new NotificationDTO();
            var responseContent = new StringContent(JsonConvert.SerializeObject(notificationDTO), Encoding.UTF8, "application/json");
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = await _notificationService.CreateNotification(notificationDTO);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Сповіщення створене", result.Message);
        }

        [Test]
        public async Task CreateNotification_LimitReached_ReturnsLimitMessage()
        {
            // Arrange
            var notificationDTO = new NotificationDTO();
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = (HttpStatusCode)405
                });

            // Act
            var result = await _notificationService.CreateNotification(notificationDTO);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Ви досягли ліміту в 3 сповіщення", result.Message);
        }

        [Test]
        public async Task CreateNotification_ServerError_ReturnsServerErrorMessage()
        {
            // Arrange
            var notificationDTO = new NotificationDTO();
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await _notificationService.CreateNotification(notificationDTO);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Сталася помилка серверу", result.Message);
        }

        //Update
        [Test]
        public async Task UpdateNotification_Successful_ReturnsSuccessMessage()
        {
            // Arrange
            var notificationDTO = new NotificationDTO();
            var responseContent = new StringContent(JsonConvert.SerializeObject(notificationDTO), Encoding.UTF8, "application/json");
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = await _notificationService.UpdateNotification(notificationDTO);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Сповіщення оновлене", result.Message);
        }

        [Test]
        public async Task UpdateNotification_NotFound_ReturnsNotFoundErrorMessage()
        {
            // Arrange
            var notificationDTO = new NotificationDTO();
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = (HttpStatusCode)404
                });

            // Act
            var result = await _notificationService.UpdateNotification(notificationDTO);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Сповіщення не зайдене", result.Message);
        }

        [Test]
        public async Task UpdateNotification_ServerError_ReturnsServerErrorMessage()
        {
            // Arrange
            var notificationDTO = new NotificationDTO();
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await _notificationService.UpdateNotification(notificationDTO);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Сталася помилка серверу", result.Message);
        }

        //Delete
        [Test]
        public async Task DeleteNotification_Successful_ReturnsSuccessMessage()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { Id = Guid.NewGuid() };
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var result = await _notificationService.DeleteNotification(notificationDTO);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Сповіщення видалене", result.Message);
        }

        [Test]
        public async Task DeleteNotification_NotFound_ReturnsNotFoundMessage()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { Id = Guid.NewGuid() };
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var result = await _notificationService.DeleteNotification(notificationDTO);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Сповіщення не зайдене", result.Message);
        }

        [Test]
        public async Task DeleteNotification_ServerError_ReturnsServerErrorMessage()
        {
            // Arrange
            var notificationDTO = new NotificationDTO { Id = Guid.NewGuid() };
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await _notificationService.DeleteNotification(notificationDTO);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Сталася помилка серверу", result.Message);
        }

        //Get
        [Test]
        public async Task GetNotifications_SuccessfulByChatId_ReturnsNotificationsForChatId()
        {
            // Arrange
            var notificationDTOs = new List<NotificationDTO>
            {
                new() { ChatId = 123L },
            };
            var responseContent = new StringContent(JsonConvert.SerializeObject(notificationDTOs), Encoding.UTF8, "application/json");
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = await _notificationService.GetNotifications(123L);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, ((IEnumerable<NotificationDTO>)result.Data).Count());
        }

        [Test]
        public async Task GetNotifications_SuccessfulByChatIdIsNull_ReturnsAllNotifications()
        {
            // Arrange
            var notificationDTOs = new List<NotificationDTO>
            {
                new() { ChatId = 123L },
                new() { ChatId = 456L }
            };
            var responseContent = new StringContent(JsonConvert.SerializeObject(notificationDTOs), Encoding.UTF8, "application/json");
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = await _notificationService.GetNotifications(null);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, ((IEnumerable<NotificationDTO>)result.Data).Count());
        }

        [Test]
        public async Task GetNotifications_ServerError_ReturnsServerErrorMessage()
        {
            // Arrange
            var notificationDTOs = new List<NotificationDTO>
            {
                new() { ChatId = 123L },
                new() { ChatId = 456L }
            };
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await _notificationService.GetNotifications(123L);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Сталася помилка серверу", result.Message);
        }
    }
}
