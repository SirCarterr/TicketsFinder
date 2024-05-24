using Microsoft.Extensions.Logging;
using Moq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TicketsFinder_API.Services;

namespace TicketsFinder_API.Tests.ServiceTests
{
    [TestFixture]
    public class TicketsServiceTests
    {
        private Mock<IWebDriver> _mockWebDriver;
        private Mock<INavigation> _mockNavigation;
        private Mock<ILogger<TicketsService>> _mockLogger;
        private TicketsService _ticketsService;
        private Mock<IWebElement> _mockFromElement;
        private Mock<IWebElement> _mockToElement;
        private Mock<IWebElement> _mockFromAutoElement;
        private Mock<IWebElement> _mockToAutoElement;
        private Mock<IWebElement> _mockDateElement;
        private Mock<IWebElement> _mockTimeElement;
        private Mock<IWebElement> _mockOptionElement;
        private Mock<IWebElement> _mockButtonElement;
        private Mock<IWebElement> _mockTrainTableElement;
        private Mock<IWebElement> _mockTrElement;
        private Mock<ITargetLocator> _mockTargetLocator;
        private Mock<IJavaScriptExecutor> _mockJsExecutor;
        private string _mockPageSource;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<TicketsService>>();
            _mockWebDriver = new Mock<IWebDriver>();
            _mockNavigation = new Mock<INavigation>();

            // Set up navigation
            _mockWebDriver.Setup(d => d.Navigate()).Returns(_mockNavigation.Object);

            // Mocking elements
            _mockFromElement = new Mock<IWebElement>();
            _mockToElement = new Mock<IWebElement>();
            _mockFromAutoElement = new Mock<IWebElement>();
            _mockToAutoElement = new Mock<IWebElement>();
            _mockDateElement = new Mock<IWebElement>();
            _mockTimeElement = new Mock<IWebElement>();
            _mockOptionElement = new Mock<IWebElement>();
            _mockButtonElement = new Mock<IWebElement>();
            _mockTrainTableElement = new Mock<IWebElement>();
            _mockTrElement = new Mock<IWebElement>();
            _mockTargetLocator = new Mock<ITargetLocator>();
            _mockJsExecutor = _mockWebDriver.As<IJavaScriptExecutor>();

            // Mock Time Element
            IList<IWebElement> options = new List<IWebElement> { _mockOptionElement.Object };

            _mockTimeElement.SetupGet<string>(_ => _.TagName).Returns("select");
            _mockTimeElement.Setup(_ => _.GetAttribute(It.Is<string>(x => x == "multiple"))).Returns((string)null);
            _mockOptionElement.SetupGet<bool>(_ => _.Selected).Returns(false);
            _mockOptionElement.SetupGet<bool>(_ => _.Enabled).Returns(true);
            _mockOptionElement.Setup(_ => _.Click());
            _mockTimeElement.Setup(_ => _.FindElements(It.IsAny<By>())).Returns(new ReadOnlyCollection<IWebElement>(options)).Verifiable();

            // Mock the WebDriver interactions
            _mockWebDriver.Setup(d => d.FindElement(By.Name("from-title"))).Returns(_mockFromElement.Object);
            _mockWebDriver.Setup(d => d.FindElement(By.Name("to-title"))).Returns(_mockToElement.Object);
            _mockWebDriver.Setup(d => d.FindElement(By.XPath("//ul[@id='from-autocomplete']/li[1]"))).Returns(_mockFromElement.Object);
            _mockWebDriver.Setup(d => d.FindElement(By.XPath("//ul[@id='to-autocomplete']/li[1]"))).Returns(_mockToElement.Object);
            _mockWebDriver.Setup(d => d.FindElement(By.XPath("//form//input[@name='date']"))).Returns(_mockDateElement.Object);
            _mockWebDriver.Setup(d => d.FindElement(By.Name("time"))).Returns(_mockTimeElement.Object);
            _mockWebDriver.Setup(d => d.FindElement(By.XPath(@"//form//div[@class='button']//button"))).Returns(_mockButtonElement.Object);
            _mockWebDriver.Setup(d => d.FindElement(By.ClassName("train-table"))).Returns(_mockTrainTableElement.Object);

            // Mock the train table rows
            var mockTrainTableRows = new ReadOnlyCollection<IWebElement>(new[] { _mockTrElement.Object });
            _mockTrainTableElement.Setup(e => e.FindElements(By.TagName("tr"))).Returns(mockTrainTableRows);

            // Mock 'no-place' row
            _mockTrElement.Setup(e => e.GetAttribute("class")).Returns("no-place");

            // Mocking expected interactions
            _mockFromElement.Setup(e => e.SendKeys(It.IsAny<string>()));
            _mockToElement.Setup(e => e.SendKeys(It.IsAny<string>()));
            _mockFromAutoElement.Setup(e => e.SendKeys(It.IsAny<string>()));
            _mockToAutoElement.Setup(e => e.SendKeys(It.IsAny<string>()));
            _mockButtonElement.Setup(e => e.Click());

            // Mock WebDriver wait
            var mockWait = new Mock<WebDriverWait>(_mockWebDriver.Object, TimeSpan.FromSeconds(5)) { CallBase = true };
            mockWait.Setup(w => w.Until(It.IsAny<Func<IWebDriver, IWebElement>>())).Returns(_mockFromElement.Object);

            // Mocking window handles
            _mockWebDriver.Setup(d => d.WindowHandles).Returns(new ReadOnlyCollection<string>(new List<string> { "window1", "window2" }));
            _mockWebDriver.Setup(d => d.SwitchTo()).Returns(_mockTargetLocator.Object);
            _mockTargetLocator.Setup(t => t.Window("window2"));

            // Mock JavaScript execution
            _mockJsExecutor.Setup(js => js.ExecuteScript("window.open();")).Returns((object)null);

            _ticketsService = new TicketsService(_mockLogger.Object, _mockWebDriver.Object);
        }

        [Test]
        public void SearchTickets_WhenCaptchaOccurs_ShouldReturnCaptchaError()
        {
            //Arrange
            _mockWebDriver.Setup(d => d.FindElement(By.ClassName("train-table"))).Throws(new WebDriverTimeoutException());

            // Act
            var result = _ticketsService.SearchTickets("Kyiv", "Lviv", "01.01.2025", "00:00");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.That(result.Message, Is.EqualTo("captcha"));
        }

        [Test]
        public void SearchTickets_ShouldReturnSiteError_WhenWebDriverTimeoutExceptionOccurs()
        {
            // Arrange
            _mockWebDriver.Setup(d => d.FindElement(By.Name("from-title"))).Throws(new WebDriverTimeoutException());

            // Act
            var result = _ticketsService.SearchTickets("Kyiv", "Lviv", "01.01.2025", "10:00");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.That(result.Message, Is.EqualTo("site error"));
        }

        [Test]
        public void SearchTickets_ShouldReturnUnexpectedError_WhenWebDriverExceptionOccurs()
        {
            // Arrange
            _mockWebDriver.Setup(d => d.Navigate()).Throws(new WebDriverException());

            // Act
            var result = _ticketsService.SearchTickets("Kyiv", "Lviv", "01.01.2025", "10:00");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.That(result.Message, Is.EqualTo("unexpected error"));
        }
    }
}
