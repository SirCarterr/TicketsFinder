using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Bot.Service;
using TicketFinder_Common;
using TicketFinder_Models;

namespace TicketsFinder_Bot.Tests
{
    [TestFixture]
    public class SearchCommandServiceTests
    {
        private Mock<ITicketService> _ticketServiceMock;
        private Mock<IUserHistoryService> _userHistoryServiceMock;
        private Mock<IValidationService> _validationServiceMock;
        private SearchCommandService _searchCommandService;
        private Mock<ITelegramBotClient> _botClientMock;

        [SetUp]
        public void Setup()
        {
            _ticketServiceMock = new Mock<ITicketService>();
            _userHistoryServiceMock = new Mock<IUserHistoryService>();
            _validationServiceMock = new Mock<IValidationService>();
            _searchCommandService = new SearchCommandService(_ticketServiceMock.Object, _userHistoryServiceMock.Object, _validationServiceMock.Object);
            _botClientMock = new Mock<ITelegramBotClient>();
        }

        [Test]
        public async Task SearchCallback_ShouldReturnCorrectResponse_WhenCalledWithValidData()
        {
            // Arrange
            var callbackQuery = new CallbackQuery
            {
                Data = "search 123-456",
                Message = new Message { Chat = new Chat { Id = 1 }, MessageId = 1 }
            };
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _searchCommandService.SearchCallback(_botClientMock.Object, callbackQuery, cancellationToken);

            // Assert
            Assert.AreEqual(2, result);
            _userHistoryServiceMock.Verify(x => x.UpdateUserHistory(It.Is<UserHistoryDTO>(u => u.ChatId == 1 && u.History == "123-456")), Times.Once);
        }

        [Test]
        public async Task SearchCommand_ShouldReturnNextStep_WhenStepIsZero()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "test" };
            var cancellationToken = new CancellationToken();
            int step = 0;

            // Act
            var result = await _searchCommandService.SearchCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task SearchCommand_ShouldReturnSameStep_WhenValidationFails()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "invalid input" };
            var cancellationToken = new CancellationToken();
            int step = 1;
            _validationServiceMock.Setup(x => x.ValidateRoute(It.IsAny<string>())).Returns(new[] { null, "Error message" });

            // Act
            var result = await _searchCommandService.SearchCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(step, result);
        }

        [Test]
        public async Task SearchCommand_ShouldReturnNextStep_WhenValidationSucceeds()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "valid input" };
            var cancellationToken = new CancellationToken();
            int step = 1;
            _validationServiceMock.Setup(x => x.ValidateRoute(It.IsAny<string>())).Returns(new[] { "from", "to" });

            // Act
            var result = await _searchCommandService.SearchCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(2, result);
            _userHistoryServiceMock.Verify(x => x.UpdateUserHistory(It.Is<UserHistoryDTO>(u => u.ChatId == 1 && u.History == "from-to")), Times.Once);
        }

        [Test]
        public async Task SearchCommand_ShouldSearchTickets_WhenFinalStepIsReached()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "valid input" };
            var cancellationToken = new CancellationToken();
            int step = SD.search_command_steps;
            _validationServiceMock.Setup(x => x.ValidateTime(It.IsAny<string>())).Returns(new[] { "12:00", null });
            _ticketServiceMock.Setup(x => x.GetTickets(It.IsAny<string[]>())).ReturnsAsync(new ResponseModelDTO
            {
                IsSuccess = true,
                Data = new List<TicketDTO> 
                { 
                    new() { Items = new() { new() { Class = "C1", Places = 10, URL = "http://localhost" } } }
                }
            });

            // Act
            var result = await _searchCommandService.SearchCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task SearchCommand_ShouldReturnErrorMessage_WhenNoTicketsFound()
        {
            // Arrange
            var message = new Message { Chat = new Chat { Id = 1 }, Text = "valid input" };
            var cancellationToken = new CancellationToken();
            int step = SD.search_command_steps;
            _validationServiceMock.Setup(x => x.ValidateTime(It.IsAny<string>())).Returns(new[] { "12:00", null });
            _ticketServiceMock.Setup(x => x.GetTickets(It.IsAny<string[]>())).ReturnsAsync(new ResponseModelDTO { IsSuccess = true, Data = new List<TicketDTO>() });

            // Act
            var result = await _searchCommandService.SearchCommand(_botClientMock.Object, message, step, cancellationToken);

            // Assert
            Assert.AreEqual(0, result);
        }
    }
}
