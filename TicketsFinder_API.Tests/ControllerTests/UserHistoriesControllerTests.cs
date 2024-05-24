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
    public class UserHistoriesControllerTests
    {
        private Mock<IUserHistoryService> _mockUserHistoryService;
        private Mock<ILogger<UserHistoriesController>> _mockLogger;
        private UserHistoriesController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUserHistoryService = new Mock<IUserHistoryService>();
            _mockLogger = new Mock<ILogger<UserHistoriesController>>();
            _controller = new UserHistoriesController(_mockUserHistoryService.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Update_UserHistoryCreated_ReturnsStatus202Accepted()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO { ChatId = 123, Id = Guid.NewGuid() };
            _mockUserHistoryService.Setup(s => s.GetHistory(It.IsAny<long>()))
                .ReturnsAsync(new UserHistoryDTO { Id = Guid.Empty });
            _mockUserHistoryService.Setup(s => s.CreateHistory(It.IsAny<UserHistoryDTO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.Update(userHistoryDTO) as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status202Accepted, result.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Update_UserHistoryUpdated_ReturnsStatus202Accepted()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO { ChatId = 123, Id = Guid.NewGuid() };
            _mockUserHistoryService.Setup(s => s.GetHistory(It.IsAny<long>()))
                .ReturnsAsync(userHistoryDTO);
            _mockUserHistoryService.Setup(s => s.UpdateHistory(It.IsAny<UserHistoryDTO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.Update(userHistoryDTO) as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status202Accepted, result.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Update_ThrowsException_ReturnsStatus500InternalServerError()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO { ChatId = 123, Id = Guid.NewGuid() };
            _mockUserHistoryService.Setup(s => s.GetHistory(It.IsAny<long>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.Update(userHistoryDTO) as StatusCodeResult;

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
        public async Task Get_UserHistoryFound_ReturnsOk()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO { ChatId = 123, Id = Guid.NewGuid() };
            _mockUserHistoryService.Setup(s => s.GetHistory(It.IsAny<long>()))
                .ReturnsAsync(userHistoryDTO);

            // Act
            var result = await _controller.Get(123) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual(userHistoryDTO, result.Value);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public async Task Get_UserHistoryNotFound_ReturnsStatus404NotFound()
        {
            // Arrange
            _mockUserHistoryService.Setup(s => s.GetHistory(It.IsAny<long>()))
                .ReturnsAsync(new UserHistoryDTO { Id = Guid.Empty });

            // Act
            var result = await _controller.Get(123) as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }
    }
}
