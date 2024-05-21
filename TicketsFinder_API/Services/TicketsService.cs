using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using TicketFinder_Models;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Services
{
    public class TicketsService : ITicketsService
    {
        private readonly string url = "https://booking.uz.gov.ua/";
        private readonly ILogger _logger;

        private readonly string[] ua = 
        {
            "Mozilla/5.0 (X11; CrOS x86_64 8172.45.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.64 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.111 Safari/537.36",
            "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:15.0) Gecko/20100101 Firefox/15.0.1",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9"
        };
        
        private IWebDriver? driver;

        public TicketsService(ILogger<TicketsService> logger)
        {
            _logger = logger;
        }

        public List<TicketDTO> SearchTickets(string from, string to, string? date, string? time)
        {
            try
            {
                ChromeOptions options = new();
                options.AddArgument($"--user-agent={ua[new Random().Next(5)]}");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--start-maximized");
                options.ImplicitWaitTimeout = TimeSpan.FromSeconds(10);
                options.PageLoadStrategy = PageLoadStrategy.Normal;

                //open site
                driver = new ChromeDriver(options);
                driver.Navigate().GoToUrl(url);

                //set "from" input value
                driver.FindElement(By.Name("from-title")).SendKeys(from);
                Thread.Sleep(250);
                driver.FindElement(By.Id("from-autocomplete")).FindElement(By.TagName("li")).Click();
                Thread.Sleep(250);

                //set "to" input value
                Thread.Sleep(250);
                driver.FindElement(By.Name("to-title")).SendKeys(to);
                Thread.Sleep(250);
                driver.FindElement(By.Id("to-autocomplete")).FindElement(By.TagName("li")).Click();

                //set date value
                if (date != null)
                {
                    DateTime dateParsed = new();
                    bool isDateParsed = DateTime.TryParseExact(date, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateParsed);
                    if (isDateParsed && dateParsed >= DateTime.Now.Date)
                    {
                        string dateParsedString = DateOnly.FromDateTime(dateParsed).ToString("yyyy-MM-dd");
                        var datePicker = driver.FindElement(By.XPath("//form//input[@name='date']"));
                        Thread.Sleep(250);
                        driver.ExecuteJavaScript($"arguments[0].value = '{dateParsedString}'", datePicker);
                    }
                }

                //set time value
                if (time != null)
                {
                    Thread.Sleep(250);
                    var timeElement = driver.FindElement(By.Name("time"));
                    var selectTime = new SelectElement(timeElement);
                    selectTime.SelectByValue(time);
                }

                //send request
                driver.FindElement(By.XPath(@"//form//div[@class='button']//button")).Click();

                //get list of train tickets
                Thread.Sleep(250);
                var list = driver.FindElements(By.XPath("//table[@class='train-table']//tr"));

                List<TicketDTO> ticketDTOs = new();
                for (int i = 1; i < list.Count; i++)
                {
                    if (list[i].GetAttribute("class").Equals("no-place"))
                        continue;

                    string num = list[i].FindElement(By.ClassName("num")).FindElement(By.TagName("div")).Text;
                    num = num.Contains("IC") ? num.Split('\n')[1] : num;
                    TicketDTO ticket = new()
                    {
                        Num = num,
                        From = list[i].FindElement(By.ClassName("station")).FindElements(By.ClassName("name"))[0].Text,
                        To = list[i].FindElement(By.ClassName("station")).FindElements(By.ClassName("name"))[1].Text,
                        Duration = TimeOnly.Parse(list[i].FindElement(By.ClassName("duration")).Text)
                    };

                    string depatureDate = list[i].FindElement(By.ClassName("date")).FindElements(By.ClassName("date"))[0].FindElements(By.TagName("span"))[1].Text.Split(", ")[1].Replace('.', '-');
                    string arrivalDate = list[i].FindElement(By.ClassName("date")).FindElements(By.ClassName("date"))[1].FindElements(By.TagName("span"))[1].Text.Split(", ")[1].Replace('.', '-');

                    depatureDate = depatureDate.Split("-")[2] + "-" + depatureDate.Split("-")[1] + "-" + depatureDate.Split("-")[0];
                    arrivalDate = arrivalDate.Split("-")[2] + "-" + arrivalDate.Split("-")[1] + "-" + arrivalDate.Split("-")[0];

                    string depatureTime = list[i].FindElement(By.ClassName("time")).FindElements(By.TagName("div"))[0].Text;
                    string arrivalTime = list[i].FindElement(By.ClassName("time")).FindElements(By.TagName("div"))[1].Text;

                    ticket.Departure = DateTime.Parse(depatureDate + 'T' + depatureTime + ":00");
                    ticket.Arrival = DateTime.Parse(arrivalDate + 'T' + arrivalTime + ":00");

                    ticket.Items = new List<TicketDTO.Item>();
                    var items = list[i].FindElements(By.ClassName("item"));
                    foreach (var item in items)
                    {
                        string seatClass = item.FindElement(By.ClassName("wagon-class")).Text;
                        TicketDTO.Item ticketItem = new()
                        {
                            Class = seatClass,
                            Places = int.Parse(item.FindElement(By.ClassName("place-count")).Text),
                            URL = driver.Url.Replace("train-list", "train-wagons") + $"&train={ticket.Num.Replace(" ", "")}&wagon_type_id={seatClass}"
                        };

                        ticket.Items.Add(ticketItem);
                    }

                    ticketDTOs.Add(ticket);
                }

                return ticketDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<TicketDTO>();
            }
            finally
            {
                driver?.Quit();
            }
        }
    }
}
