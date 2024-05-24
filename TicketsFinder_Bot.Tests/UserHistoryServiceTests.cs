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
    public class UserHistoryServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private UserHistoryService _userHistoryService;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            _userHistoryService = new UserHistoryService(_httpClient);
        }

        //Get
        [Test]
        public async Task GetUserHistory_Successful_ReturnsUserHistory()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO() { ChatId = 123L };
            var responseContent = new StringContent(JsonConvert.SerializeObject(userHistoryDTO), Encoding.UTF8, "application/json");
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = await _userHistoryService.GetUserHistory(123L);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(userHistoryDTO.ChatId, ((UserHistoryDTO)result.Data).ChatId);
        }

        [Test]
        public async Task GetUserHistory_NotFound_ReturnsNotFoundErrorMessage()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO() { ChatId = 123L };
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                });

            // Act
            var result = await _userHistoryService.GetUserHistory(123L);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Історія пуста", result.Message);
        }

        [Test]
        public async Task GetUserHistory_ServerError_ReturnsServerErrorMessage()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO() { ChatId = 123L };
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await _userHistoryService.GetUserHistory(123L);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Сталася помилка серверу", result.Message);
        }

        //Update
        [Test]
        public async Task UpdateUserHistory_Succesful_ReturnsSuccessMessage()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO() { ChatId = 123L };
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                });

            // Act
            var result = await _userHistoryService.UpdateUserHistory(userHistoryDTO);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task UpdateUserHistory_ServerError_ReturnsServerErrorMessage()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO() { ChatId = 123L };
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                });

            // Act
            var result = await _userHistoryService.UpdateUserHistory(userHistoryDTO);

            // Assert
            Assert.IsFalse(result.IsSuccess);
        }
    }
}
