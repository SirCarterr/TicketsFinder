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
    public class TicketServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private TicketService _ticketService;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            _ticketService = new TicketService(_httpClient);
        }

        [Test]
        public async Task GetTickets_Successful_ReturnsSuccessMessage()
        {
            // Arrange
            var ticketDTOs = new List<TicketDTO>() { new() { From = "from1", To = "To1" } };
            var responseContent = new StringContent(JsonConvert.SerializeObject(ticketDTOs), Encoding.UTF8, "application/json");
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = await _ticketService.GetTickets(new string[4] { "from1", "from2", "", "" });

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, ((List<TicketDTO>)result.Data).Count);
        }

        [Test]
        public async Task GetTickets_TooManyRequests_ReturnsTooManyRequestsErrorMessage()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.TooManyRequests,
                });

            // Act
            var result = await _ticketService.GetTickets(new string[4] { "from1", "from2", "", "" });

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Сервіс перенавантажений, спробуйте ще раз через пару хвилин", result.Message);
        }

        [Test]
        public async Task GetTickets_BadGateway_ReturnsBadGatewayErrorMessage()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                });

            // Act
            var result = await _ticketService.GetTickets(new string[4] { "from1", "from2", "", "" });

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Неможливо зробити пошук, спробуйте ще раз пізніше", result.Message);
        }

        [Test]
        public async Task GetTickets_GatewayTimeout_ReturnsGatewayTimeoutErrorMessage()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.GatewayTimeout,
                });

            // Act
            var result = await _ticketService.GetTickets(new string[4] { "from1", "from2", "", "" });

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Час запиту вийшов, спробуйте ще раз через пару хвилин", result.Message);
        }

        [Test]
        public async Task GetTickets_InternalServerError_ReturnsInternalServerErrorMessage()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                });

            // Act
            var result = await _ticketService.GetTickets(new string[4] { "from1", "from2", "", "" });

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Сталась помилка при пошуку квитків", result.Message);
        }
    }
}
