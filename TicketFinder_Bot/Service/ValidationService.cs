﻿using System;
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
            if (string.IsNullOrEmpty(input) || input.Equals("Сьогодні"))
            {
                date[0] = DateTime.Now.Date.ToString("dd.MM.yyyy");
                return date;
            }

            if (input.Equals("Завтра"))
            {
                date[0] = DateTime.Now.Date.AddDays(1).ToString("dd.MM.yyyy");
                return date;
            }

            if (input.Equals("Післязавтра"))
            {
                date[0] = DateTime.Now.Date.AddDays(1).ToString("dd.MM.yyyy");
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

        public string[] ValidateRoute(string input)
        {
            string[] route = new string[2];
            if (string.IsNullOrEmpty(input))
            {
                route[0] = "Неможливо зробити пошук. Введіть місця маршруту";
            }

            Match match = Regex.Match(input, @"\s*(?<from>\w+)\s*[-\s]\s*(?<to>\w+)\s*");
            if (match.Success)
            {
                route[0] = match.Groups[1].Value;
                route[1] = match.Groups[2].Value;
            }
            else
                route[0] = "Невірний формат вводу, спробуйте ще";
            return route;
        }
    }
}