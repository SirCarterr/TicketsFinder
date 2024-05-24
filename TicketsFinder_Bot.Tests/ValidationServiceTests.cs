using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketFinder_Bot.Service;

namespace TicketsFinder_Bot.Tests
{
    [TestFixture]
    public class ValidationServiceTests
    {
        private ValidationService _validationService;

        [SetUp]
        public void Setup()
        {
            _validationService = new ValidationService();
        }

        [Test]
        [TestCase("сьогодні", ExpectedResult = new[] { "24.05.2024", null })] // Adjust the date accordingly
        [TestCase("завтра", ExpectedResult = new[] { "25.05.2024", null })] // Adjust the date accordingly
        [TestCase("післязавтра", ExpectedResult = new[] { "26.05.2024", null })] // Adjust the date accordingly
        [TestCase("25.12.2024", ExpectedResult = new[] { "25.12.2024", null })]
        [TestCase("25/12/2024", ExpectedResult = new[] { null, "Невірний формат дати, пробуйте ще" })]
        public string[] ValidateDateTests(string input)
        {
            return _validationService.ValidateDate(input);
        }

        [Test]
        [TestCase("парні", ExpectedResult = new[] { "парні", null })]
        [TestCase("непарні", ExpectedResult = new[] { "непарні", null })]
        [TestCase("будні", ExpectedResult = new[] { "будні", null })]
        [TestCase("вихідні", ExpectedResult = new[] { "вихідні", null })]
        [TestCase("понеділок, вівторок", ExpectedResult = new[] { "понеділок, вівторок", null })]
        [TestCase("пн, вт", ExpectedResult = new[] { null, "Невірний формат вводу, спробуйте ще" })]
        public string[] ValidateDaysTests(string input)
        {
            return _validationService.ValidateDays(input);
        }

        [Test]
        [TestCase("30", ExpectedResult = new[] { "30", null })]
        [TestCase("91", ExpectedResult = new[] { null, "Ліміт числа днів наперед не повинен перевищувати 90 днів" })]
        [TestCase("string", ExpectedResult = new[] { null, "Невірний формат вводу, спробуйте ще" })]
        public string[] ValidateDaysNumberTests(string input)
        {
            return _validationService.ValidateDaysNumber(input);
        }

        [Test]
        [TestCase("Київ - Львів", ExpectedResult = new[] { "Київ", "Львів" })]
        [TestCase("Київ-Львів", ExpectedResult = new[] { "Київ", "Львів" })]
        [TestCase("", ExpectedResult = new[] { null, "Неможливо зробити пошук. Введіть місця маршруту" })]
        [TestCase("Київ", ExpectedResult = new[] { null, "Невірний формат вводу, спробуйте ще" })]
        public string[] ValidateRouteTests(string input)
        {
            return _validationService.ValidateRoute(input);
        }

        [Test]
        [TestCase("12:30", ExpectedResult = new[] { "12:30", null })]
        [TestCase("", ExpectedResult = new[] { "00:00", null })]
        [TestCase("12-30", ExpectedResult = new[] { null, "Невірний формат вводу, спробуйте ще" })]
        public string[] ValidateTimeTests(string input)
        {
            return _validationService.ValidateTime(input);
        }
    }
}
