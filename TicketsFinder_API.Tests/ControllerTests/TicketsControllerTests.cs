using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
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
    public class TicketsControllerTests
    {
        private Mock<ITicketsService> _mockTicketsService;
        private Mock<ILogger<TicketsController>> _mockLogger;
        private TicketsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockTicketsService = new Mock<ITicketsService>();
            _mockLogger = new Mock<ILogger<TicketsController>>();
            _controller = new TicketsController(_mockTicketsService.Object, _mockLogger.Object);
        }

        [Test]
        public void SearchTickets_Successful_ReturnsOk()
        {
            // Arrange
            var response = new ResponseModelDTO { IsSuccess = true, Data = "some data" };
            _mockTicketsService.Setup(s => s.SearchTickets(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response);

            // Act
            var result = _controller.SearchTickets("Kyiv", "Lviv", "2022-01-01", "10:00") as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual(response.Data, result.Value);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public void SearchTickets_SiteError_ReturnsBadGateway()
        {
            // Arrange
            var response = new ResponseModelDTO { IsSuccess = false, Message = "site error" };
            _mockTicketsService.SetupSequence(s => s.SearchTickets(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response);

            // Act
            var result = _controller.SearchTickets("Kyiv", "Lviv", "2022-01-01", "10:00") as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status502BadGateway, result.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public void SearchTickets_UnexpectedError_ReturnsInternalServerError()
        {
            // Arrange
            var response = new ResponseModelDTO { IsSuccess = false, Message = "unexpected error" };
            _mockTicketsService.SetupSequence(s => s.SearchTickets(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response);

            // Act
            var result = _controller.SearchTickets("Kyiv", "Lviv", "2022-01-01", "10:00") as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public void SearchTickets_ThreeAttempts_Fails_ReturnsGatewayTimeout()
        {
            // Arrange
            var response = new ResponseModelDTO { IsSuccess = false, Message = "unknown error" };
            _mockTicketsService.SetupSequence(s => s.SearchTickets(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response)
                .Returns(response)
                .Returns(response);

            // Act
            var result = _controller.SearchTickets("Kyiv", "Lviv", "2022-01-01", "10:00") as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status504GatewayTimeout, result.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }
    }
}
