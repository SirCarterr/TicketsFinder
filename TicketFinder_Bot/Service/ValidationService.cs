using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TicketFinder_Bot.Service.IService;

namespace TicketFinder_Bot.Service
{
    public class ValidationService : IValidationService
    {
        public string[] ValidateDate(string input)
        {
            string[] date = new string[2];
            if (string.IsNullOrEmpty(input) || input.ToLower().Equals("сьогодні"))
            {
                date[0] = DateTime.Now.Date.ToString("dd.MM.yyyy");
                return date;
            }

            if (input.ToLower().Equals("завтра"))
            {
                date[0] = DateTime.Now.Date.AddDays(1).ToString("dd.MM.yyyy");
                return date;
            }

            if (input.ToLower().Equals("післязавтра"))
            {
                date[0] = DateTime.Now.Date.AddDays(2).ToString("dd.MM.yyyy");
                return date;
            }

            Match match = Regex.Match(input, @"\d\d\.\d\d\.\d\d\d\d");
            if (match.Success)
            {
                date[0] = match.Value;
            }
            else
            {
                date[1] = "Невірний формат дати, пробуйте ще";
            }
            return date;
        }

        public string[] ValidateDays(string input)
        {
            string[] days = new string[2];
            Match match1 = Regex.Match(input.ToLower(), @"(парні|непарні|будні|вихідні)");
            if (match1.Success)
            {
                days[0] = match1.Value;
                return days;
            }

            Match match2 = Regex.Match(input.ToLower(), @"(понеділок|вівторок|середа|четвер|п'ятниця)(,\s(понеділок|вівторок|середа|четвер|п'ятниця))*");
            if (match2.Success)
            {
                days[0] = match2.Value;
            }
            else
            {
                days[1] = "Невірний формат вводу, спробуйте ще";
            }
            return days;
        }

        public string[] ValidateDaysNumber(string input)
        {
            string[] daysNumber = new string[2];
            bool isParsed = int.TryParse(input, out int temp);
            if (isParsed)
            {
                if (temp > 90)
                {
                    daysNumber[1] = "Ліміт числа днів наперед не повинен перевищувати 90 днів";
                }
                else
                {
                    daysNumber[0] = input;
                }
            }
            else
            {
                daysNumber[1] = "Невірний формат вводу, спробуйте ще";
            }
            return daysNumber;
        }

        public string[] ValidateRoute(string input)
        {
            string[] route = new string[2];
            if (string.IsNullOrEmpty(input))
            {
                route[1] = "Неможливо зробити пошук. Введіть місця маршруту";
            }

            Match match = Regex.Match(input, @"\s*(?<from>\w+)\s*[-\s]\s*(?<to>\w+)\s*");
            if (match.Success)
            {
                route[0] = match.Groups[1].Value;
                route[1] = match.Groups[2].Value;
            }
            else
                route[1] = "Невірний формат вводу, спробуйте ще";
            return route;
        }

        public string[] ValidateTime(string input)
        {
            string[] time = new string[2];
            if (string.IsNullOrEmpty(input))
            {
                time[0] = "00:00";
            }

            Match match = Regex.Match(input, @"\d\d:\d\d");
            if (match.Success)
            {
                time[0] = input;
            }
            else
            {
                time[1] = "Невірний формат вводу, спробуйте ще";
            }
            return time;
        }
    }
}
