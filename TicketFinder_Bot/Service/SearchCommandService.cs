using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TicketFinder_Bot.Helper;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Common;
using TicketFinder_Models;

namespace TicketFinder_Bot.Service
{
    public class SearchCommandService : ISearchCommandService
    {
        private readonly ITicketService _ticketService;
        private readonly IUserHistoryService _userHistoryService; 
        private readonly IValidationService _validationService;

        public SearchCommandService(ITicketService ticketService, IUserHistoryService userHistoryService, IValidationService validationService)
        {
            _ticketService = ticketService;
            _userHistoryService = userHistoryService;
            _validationService = validationService;
        }

        public async Task<int> SearchCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string data = callbackQuery.Data!.Split(" ")[1];
            string[] route = data.Split("-");
            _ticketService.RequestSearch[0] = route[0];
            _ticketService.RequestSearch[1] = route[1];
            await _userHistoryService.UpdateUserHistory(new() { ChatId = callbackQuery.Message!.Chat.Id, History = $"{route[0]}-{route[1]}" });

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Виконую пошук: <b>{route[0]} -> {route[1]}</b>",
                parseMode: ParseMode.Html,
                replyToMessageId: callbackQuery.Message.MessageId,
                disableNotification: true,
                cancellationToken: cancellationToken);
            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: SD.search_command_messages[1],
                parseMode: ParseMode.Html,
                replyMarkup: ReplyKeyboards.searchReplyMarkups[1],
                disableNotification: true,
                cancellationToken: cancellationToken);
            return 2;
        }

        public async Task<int> SearchCommand(ITelegramBotClient botClient, Message message, int step, CancellationToken cancellationToken)
        {
            if (step == 0)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: SD.search_command_messages[step],
                    parseMode: ParseMode.Html,
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return ++step;
            }

            (bool isSuccessful, string[] data) = ValidateInput(message.Text!, step);
            if (isSuccessful)
            {
                if (step == 1)
                {
                    _ticketService.RequestSearch[step - 1] = data[0];
                    _ticketService.RequestSearch[step] = data[1];
                    await _userHistoryService.UpdateUserHistory(new() { ChatId = message.Chat.Id, History = $"{data[0]}-{data[1]}" });
                }
                else
                {
                    _ticketService.RequestSearch[step] = data[0];
                }

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: SD.search_command_messages[step],
                    parseMode: ParseMode.Html,
                    replyMarkup: ReplyKeyboards.searchReplyMarkups[step],
                    disableNotification: true,
                    cancellationToken: cancellationToken);

                if (step == SD.search_command_steps)
                {
                    List<TicketDTO> tickets = await _ticketService.GetTickets();
                    if (!tickets.Any())
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Немає квитків на вказаний маршрут за введеними даними",
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Знайдені квитки на вказаний маршрут:",
                            cancellationToken: cancellationToken);

                        foreach (TicketDTO ticket in tickets)
                        {
                            InlineKeyboardButton[] inlineKeyboardButtons = new InlineKeyboardButton[ticket.Items.Count];
                            for (int i = 0; i < ticket.Items.Count; i++)
                            {
                                inlineKeyboardButtons[i] = InlineKeyboardButton.WithUrl($"{ticket.Items[i].Class}: {ticket.Items[i].Places}", ticket.Items[i].URL);
                            }

                            InlineKeyboardMarkup inlineKeyboardMarkup = new(inlineKeyboardButtons);

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: SD.ConstructTicketMessage(ticket),
                                parseMode: ParseMode.Html,
                                replyMarkup: inlineKeyboardMarkup,
                                cancellationToken: cancellationToken);
                        }
                    }
                    _ticketService.RequestSearch = new string[4];
                    return 0;
                }
                return ++step;
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Помилка:\n" + data[1],
                cancellationToken: cancellationToken);
            return step;
        }

        private (bool isSuccessful, string[] data) ValidateInput(string message, int commandStep)
        {
            string[] result = commandStep switch
            {
                1 => _validationService.ValidateRoute(message),
                2 => _validationService.ValidateDate(message),
                3 => _validationService.ValidateTime(message),
                _ => new string[2]
            };

            if (string.IsNullOrEmpty(result[0]))
                return (false, result);
            return (true, result);
        }
    }
}
