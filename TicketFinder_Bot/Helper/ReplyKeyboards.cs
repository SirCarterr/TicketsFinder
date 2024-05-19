using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TicketFinder_Models;

namespace TicketFinder_Bot.Helper
{
    public class ReplyKeyboards
    {
        public static readonly Dictionary<int, IReplyMarkup> searchReplyMarkups = new()
        {
            { 1, new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] {"Сьогодні"},
                    new KeyboardButton[] {"Завтра"},
                    new KeyboardButton[] {"Післязавтра"},
                })
            },
            { 2, new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] {"00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00"},
                    new KeyboardButton[] {"08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00"},
                    new KeyboardButton[] {"16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00"},
                })
            },
            { 3, new ReplyKeyboardRemove() }
        };

        public static readonly ReplyKeyboardMarkup searchDateMarkup = new(new[]
        {
            new KeyboardButton[] {"Сьогодні"},
            new KeyboardButton[] {"Завтра"},
            new KeyboardButton[] {"Післязавтра"},
        });

        public static readonly ReplyKeyboardMarkup searchTimeMarkup = new(new[]
        {
            new KeyboardButton[] {"00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00"},
            new KeyboardButton[] {"08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00"},
            new KeyboardButton[] {"16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00"},
        });

        public static InlineKeyboardMarkup GetUserHistoryMarkup(UserHistoryDTO userHistoryDTO)
        {
            string[] search = userHistoryDTO.History.Split(';');

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[search.Length][];
            for (int i = 0; i < search.Length; i++)
            {
                string[] route = search[i].Split("-");
                buttons[i] = new[] { InlineKeyboardButton.WithCallbackData(text: $"{route[0]} -> {route[1]}", callbackData: $"search {search[i]}") }; 
            }
            
            return new InlineKeyboardMarkup(buttons);
        }
    }
}
