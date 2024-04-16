using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using TicketFinder_Models;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Services
{
    public class TicketsService : ITicketsService
    {
        private readonly string url = "https://booking.uz.gov.ua/";
        private IWebDriver? driver;

        public List<TicketDTO> SearchTickets(string from, string to, string? date, string? time)
        {
            try
            {
                //open site
                driver = new ChromeDriver();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                driver.Navigate().GoToUrl(url);

                //set "from" input value
                driver.FindElement(By.Name("from-title")).SendKeys(from);
                driver.FindElement(By.Id("from-autocomplete")).FindElement(By.TagName("li")).Click();

                //set "to" input value
                driver.FindElement(By.Name("to-title")).SendKeys(to);
                driver.FindElement(By.Id("to-autocomplete")).FindElement(By.TagName("li")).Click();

                //set date value
                if (date != null)
                {
                    var datePicker = driver.FindElement(By.Name("date-hover"));
                    datePicker.Clear();
                    datePicker.SendKeys(date);
                    datePicker.Click();
                }

                //set time value
                if (time != null)
                {
                    var timeElement = driver.FindElement(By.Name("time"));
                    var selectTime = new SelectElement(timeElement);
                    selectTime.SelectByValue(time);
                }

                //send request
                driver.FindElement(By.XPath(@"//form//div[@class='button']//button")).Click();

                //get list of train tickets
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

                    //var cultureInfo = new CultureInfo("uk-UA");
                    ticket.Departure = DateTime.Parse(depatureDate + 'T' + depatureTime + ":00");
                    ticket.Arrival = DateTime.Parse(arrivalDate + 'T' + arrivalTime + ":00");

                    ticket.Items = new List<TicketDTO.Item>();
                    var items = list[i].FindElements(By.ClassName("item"));
                    foreach (var item in items)
                    {
                        string seatClass = item.FindElement(By.ClassName("wagon-class")).Text;
                        //string seatClass = item.FindElement(By.ClassName("wagon-class")).Text.Replace('\"', ' ');
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
                return new List<TicketDTO>();
            }
            finally
            {
                driver?.Quit();
            }
        }
    }
}
