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
        private static readonly ReplyKeyboardMarkup dateReplyMarkup = new(new[]
        {
            new KeyboardButton[] {"Сьогодні"},
            new KeyboardButton[] {"Завтра"},
            new KeyboardButton[] {"Післязавтра"},
        });

        private static readonly ReplyKeyboardMarkup timeReplyMarkup = new(new[]
        {
            new KeyboardButton[] {"00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00"},
            new KeyboardButton[] {"08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00"},
            new KeyboardButton[] {"16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00"},
        });

        private static readonly ReplyKeyboardMarkup daysReplyMarkup = new(new[]
        {
            new KeyboardButton[] {"Парні"},
            new KeyboardButton[] {"Непарні"},
            new KeyboardButton[] {"Будні"},
            new KeyboardButton[] {"Вихідні"},
        });

        public static readonly ReplyKeyboardMarkup deleteReplyMarkup = new(new[]
        {
            new KeyboardButton[] {"Так", "Ні"},
        });

        public static readonly Dictionary<int, IReplyMarkup> searchReplyMarkups = new()
        {
            { 1, dateReplyMarkup },
            { 2, timeReplyMarkup },
            { 3, new ReplyKeyboardRemove() }
        };

        public static readonly Dictionary<int, IReplyMarkup> notificationReplyMarkups = new()
        {
            { 1, timeReplyMarkup },
            { 2, daysReplyMarkup },
            { 3, timeReplyMarkup },
            { 4, new ReplyKeyboardRemove() }
        };

        public static InlineKeyboardMarkup GetTicketReplyMarkup(TicketDTO ticketDTO)
        {
            InlineKeyboardButton[] inlineKeyboardButtons = new InlineKeyboardButton[ticketDTO.Items.Count];
            for (int i = 0; i < ticketDTO.Items.Count; i++)
            {
                inlineKeyboardButtons[i] = InlineKeyboardButton.WithUrl($"{ticketDTO.Items[i].Class}: {ticketDTO.Items[i].Places}", ticketDTO.Items[i].URL);
            }

            return new(inlineKeyboardButtons);
        }

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

        public static InlineKeyboardMarkup GetNotificationMarkup(NotificationDTO notificationDTO)
        {
            return new(new[]
            {
                new [] { InlineKeyboardButton.WithCallbackData(text: "Редагувати", callbackData: $"notification-update {notificationDTO.Id}") },
                new [] { InlineKeyboardButton.WithCallbackData(text: "Видалити", callbackData: $"notification-delete {notificationDTO.Id}") }
            }); 
        }
    }
}
