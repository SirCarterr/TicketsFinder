using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketFinder_Models;
using TicketsFinder_API.Models;
using TicketsFinder_API.Models.Data;
using TicketsFinder_API.Services;

namespace TicketsFinder_API.Tests.ServiceTests
{
    [TestFixture]
    public class UserHistoryServiceTests
    {
        private AppDbContext _dbContext;
        private Mock<IMapper> _mockMapper;
        private UserHistotyService _userHistotyService;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

            _dbContext = new AppDbContext(options);
            _mockMapper = new Mock<IMapper>();
            _userHistotyService = new UserHistotyService(_dbContext, _mockMapper.Object);

            _dbContext.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task CreateUserHistory_ShouldReturn1_WhenUserHistoryIsCreated()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO { ChatId = 123L, History = "Kyiv-Lviv" };
            var userHistory = new UserHistory() { ChatId = 123L, History = "Kyiv-Lviv" };
            _mockMapper.Setup(m => m.Map<UserHistoryDTO, UserHistory>(userHistoryDTO)).Returns(userHistory);

            // Act
            var result = await _userHistotyService.CreateHistory(userHistoryDTO);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            Assert.That(await _dbContext.UserHistories.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetUserHistory_ShouldReturnUserHistory_ForSpecificChatId()
        {
            // Arrange
            var chatId = 123L;
            _dbContext.UserHistories.Add(new UserHistory { ChatId = 123L, History = "Kyiv-Lviv" });
            await _dbContext.SaveChangesAsync();

            var userHistoryDTO = new UserHistoryDTO { ChatId = 123L, History = "Kyiv-Lviv" };
            _mockMapper.Setup(m => m.Map<UserHistory, UserHistoryDTO>(It.IsAny<UserHistory>())).Returns(userHistoryDTO);

            // Act
            var result = await _userHistotyService.GetHistory(chatId);

            // Assert
            Assert.That(result.ChatId, Is.EqualTo(chatId));
        }

        [Test]
        public async Task GetUserHistory_ShouldReturnEmptyUserHistory_WhenUserHistoryNotFound()
        {
            // Arrange
            var chatId = 123L;
            _dbContext.UserHistories.Add(new UserHistory { ChatId = 456L, History = "Kyiv-Odesa" });
            await _dbContext.SaveChangesAsync();

            var userHistoryDTO = new UserHistoryDTO { ChatId = 123L, History = "Kyiv-Lviv" };
            _mockMapper.Setup(m => m.Map<UserHistory, UserHistoryDTO>(It.IsAny<UserHistory>())).Returns(userHistoryDTO);

            // Act
            var result = await _userHistotyService.GetHistory(chatId);

            // Assert
            Assert.That(result.ChatId, Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateUserHistory_ShouldReturn1_WhenUserHistoryIsUpdated()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO { Id = Guid.NewGuid(), ChatId = 123L, History = "Kharkiv-Odesa" };
            var userHistory = new UserHistory { Id = userHistoryDTO.Id, ChatId = 123L, History = "Kyiv-Lviv" };
            _dbContext.UserHistories.Add(userHistory);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userHistotyService.UpdateHistory(userHistoryDTO);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            var updatedNotification = await _dbContext.UserHistories.FirstOrDefaultAsync(n => n.Id == userHistoryDTO.Id);
            Assert.That(updatedNotification?.History, Is.EqualTo("Kharkiv-Odesa;Kyiv-Lviv"));
        }

        [Test]
        public async Task UpdateUserHistory_ShouldReturn0_WheUserHistoryIsNotFound()
        {
            // Arrange
            var userHistoryDTO = new UserHistoryDTO { Id = Guid.NewGuid(), ChatId = 123L, History = "Kyiv-Lviv" };

            // Act
            var result = await _userHistotyService.UpdateHistory(userHistoryDTO);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }
    }
}
