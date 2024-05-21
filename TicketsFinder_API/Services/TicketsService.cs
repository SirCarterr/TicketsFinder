using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Collections.ObjectModel;
using System.Globalization;
using TicketFinder_Models;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Services
{
    public class TicketsService : ITicketsService
    {
        private readonly string url = "https://booking.uz.gov.ua/";
        private readonly ILogger _logger;

        private readonly Random random;
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
            random = new Random();
        }

        public ResponseModelDTO SearchTickets(string from, string to, string? date, string? time)
        {
            bool isCaptcha = false;
            try
            {
                //setup chrome options
                ChromeOptions options = new();
                options.AddArgument($"--user-agent={ua[random.Next(ua.Length)]}");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--start-maximized");
                options.PageLoadStrategy = PageLoadStrategy.Normal;

                //open site
                driver = new ChromeDriver(options);
                driver.Navigate().GoToUrl(url);
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(5));

                //set "from" input value
                wait.Until(ExpectedConditions.ElementExists(By.Name("from-title"))).SendKeys(from);
                wait.Until(ExpectedConditions.ElementExists(By.XPath("//ul[@id='from-autocomplete']/li[1]"))).Click();

                //set "to" input value
                wait.Until(ExpectedConditions.ElementExists(By.Name("to-title"))).SendKeys(to);
                wait.Until(ExpectedConditions.ElementExists(By.XPath("//ul[@id='to-autocomplete']/li[1]"))).Click();

                //set date value
                if (date != null)
                {
                    DateTime dateParsed = new();
                    bool isDateParsed = DateTime.TryParseExact(date, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateParsed);
                    if (isDateParsed && dateParsed >= DateTime.Now.Date)
                    {
                        string dateParsedString = DateOnly.FromDateTime(dateParsed).ToString("yyyy-MM-dd");
                        var datePicker = wait.Until(ExpectedConditions.ElementExists(By.XPath("//form//input[@name='date']")));
                        driver.ExecuteJavaScript($"arguments[0].value = '{dateParsedString}'", datePicker);
                    }
                }

                //set time value
                if (time != null)
                {
                    var timeElement = wait.Until(ExpectedConditions.ElementExists(By.Name("time")));
                    var selectTime = new SelectElement(timeElement);
                    selectTime.SelectByValue(time);
                }

                //send request
                wait.Until(ExpectedConditions.ElementExists(By.XPath(@"//form//div[@class='button']//button"))).Click();
                isCaptcha = true;

                //get list of train tickets         
                wait.Timeout = TimeSpan.FromSeconds(15);
                var list = wait.Until(ExpectedConditions.ElementExists(By.ClassName("train-table"))).FindElements(By.TagName("tr"));
                isCaptcha = false;

                //return data in a list of objects
                return new() { IsSuccess = true, Data = GetTicketsList(list) };
            }
            catch (WebDriverTimeoutException)
            {
                if (isCaptcha)
                {
                    _logger.LogWarning("Captcha occured");
                    return new() { IsSuccess = false, Message = "captcha" };
                }
                _logger.LogError("Booking site's server error");
                return new() { IsSuccess =  false, Message = "site error" };
            }
            catch (WebDriverException ex)
            {
                _logger.LogError($"Driver error: {ex.Message}");
                return new() { IsSuccess = false, Message = "unexpected error" };
            }
            finally
            {
                driver?.Quit();
            }
        }

        private List<TicketDTO> GetTicketsList(ReadOnlyCollection<IWebElement> list)
        {
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
                        URL = driver!.Url.Replace("train-list", "train-wagons") + $"&train={ticket.Num.Replace(" ", "")}&wagon_type_id={seatClass}"
                    };

                    ticket.Items.Add(ticketItem);
                }

                ticketDTOs.Add(ticket);
            }
            return ticketDTOs;
        }
    }
}
