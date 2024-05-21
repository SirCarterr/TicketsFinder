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
    public class NotificationCommandService : INotificationCommandService
    {
        private readonly INotificationService _notificationService;
        private readonly IValidationService _validationService;

        public NotificationCommandService(INotificationService notificationService, IValidationService validationService)
        {
            _notificationService = notificationService;
            _validationService = validationService;

        }

        public async Task<int> CreateNotificationCommand(ITelegramBotClient botClient, Message message, int step, CancellationToken cancellationToken)
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
                switch (step)
                {
                    case 1:
                        _notificationService.RequestNotificationDTO.From = data[0];
                        _notificationService.RequestNotificationDTO.To = data[1];
                        break;
                    case 2:
                        _notificationService.RequestNotificationDTO.TicketTime = data[0];
                        break;
                    case 3:
                        _notificationService.RequestNotificationDTO.Days = data[0];
                        break;
                    case 4:
                        _notificationService.RequestNotificationDTO.Time = data[0];
                        break;
                    case 5:
                        _notificationService.RequestNotificationDTO.DaysToTrip = int.Parse(data[0]);
                        break;
                }

                if (step == SD.notificationCreate_command_steps)
                {
                    _notificationService.RequestNotificationDTO.ChatId = message.Chat.Id;
                    ResponseModelDTO response = await _notificationService.CreateNotification();

                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: response.Message!,
                        parseMode: ParseMode.Html,
                        disableNotification: true,
                        cancellationToken: cancellationToken);

                    _notificationService.RequestNotificationDTO = new();
                    return 0;
                }
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: SD.notification_command_messages[step],
                    parseMode: ParseMode.Html,
                    replyMarkup: ReplyKeyboards.notificationReplyMarkups[step],
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return ++step;
            }
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Помилка:\n" + data[1],
                cancellationToken: cancellationToken);
            return step;
        }

        public async Task<int> DeleteNotificationCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string data = callbackQuery.Data!.Split(" ")[1];
            ResponseModelDTO response = await _notificationService.GetNotifications(callbackQuery.Message!.Chat.Id);
            if (response != null)
            {
                NotificationDTO? notificationDTO = ((IEnumerable<NotificationDTO>)response.Data!).FirstOrDefault(n => n.Id == Guid.Parse(data));
                if (notificationDTO != null)
                {
                    _notificationService.RequestNotificationDTO.Id = notificationDTO.Id;

                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: $"Ви дійсно хочете видалити це сповіщення?",
                        parseMode: ParseMode.Html,
                        replyMarkup: ReplyKeyboards.deleteReplyMarkup,
                        disableNotification: true,
                        cancellationToken: cancellationToken);
                    return 1;
                }
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: $"Це сповіщення вже не існує",
                    parseMode: ParseMode.Html,
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return 1;
            }
            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Неможливо видалити це сповіщення",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: cancellationToken);
            return 0;
        }

        public async Task<int> DeleteNotificationCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text!.Trim().ToLower().Equals("так"))
            {
                ResponseModelDTO response = await _notificationService.DeleteNotification();

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: response.Message!,
                    parseMode: ParseMode.Html,
                    replyMarkup: new ReplyKeyboardRemove(),
                    disableNotification: true,
                    cancellationToken: cancellationToken);

                _notificationService.RequestNotificationDTO = new();
                return 0;
            }
            if (message.Text!.Trim().ToLower().Equals("ні"))
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Команда відмінена",
                    parseMode: ParseMode.Html,
                    replyMarkup: new ReplyKeyboardRemove(),
                    disableNotification: true,
                    cancellationToken: cancellationToken);

                _notificationService.RequestNotificationDTO = new();
                return 0;
            }
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Невірний формат вводу, спробуйте ще",
                parseMode: ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove(),
                disableNotification: true,
                cancellationToken: cancellationToken);
            return 1;
        }

        public async Task GetNotificationsCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ResponseModelDTO response = await _notificationService.GetNotifications(message.Chat.Id);
            if (response.IsSuccess)
            {
                IEnumerable<NotificationDTO> notificationDTOs = (IEnumerable<NotificationDTO>)response.Data!;
                if (notificationDTOs.Any())
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: SD.notifications_any,
                        disableNotification: true,
                        cancellationToken: cancellationToken);
                    foreach (NotificationDTO notificationDTO in notificationDTOs)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: SD.ConstructNotificationMessage(notificationDTO),
                            parseMode: ParseMode.Html,
                            replyMarkup: ReplyKeyboards.GetNotificationMarkup(notificationDTO),
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                    }
                    return;
                }
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: SD.notifications_empty,
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return;
            }
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response.Message!,
                disableNotification: true,
                cancellationToken: cancellationToken);
        }

        public async Task<int> UpdateNotificationCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string data = callbackQuery.Data!.Split(" ")[1];
            ResponseModelDTO response = await _notificationService.GetNotifications(callbackQuery.Message!.Chat.Id);
            if (response != null)
            {
                NotificationDTO? notificationDTO = ((IEnumerable<NotificationDTO>)response.Data!).FirstOrDefault(n => n.Id == Guid.Parse(data));
                if (notificationDTO != null)
                {
                    _notificationService.RequestNotificationDTO = notificationDTO;

                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: $"Виконую редагування обраного сповіщення:\nЩоб пропустити поле введіть <b>\"-\"</b>",
                        parseMode: ParseMode.Html,
                        disableNotification: true,
                        cancellationToken: cancellationToken);
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: SD.notification_command_messages[0],
                        parseMode: ParseMode.Html,
                        disableNotification: true,
                        cancellationToken: cancellationToken);
                    return 1;
                }
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: $"Це сповіщення вже не існує",
                    parseMode: ParseMode.Html,
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return 0;
            }
            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Неможливо оновити це сповіщення",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: cancellationToken);
            return 0;
        }

        public async Task<int> UpdateNotificationCommand(ITelegramBotClient botClient, Message message, int step, CancellationToken cancellationToken)
        {
            if (!message.Text!.Trim().Equals("-"))
            {
                (bool isSuccessful, string[] data) = ValidateInput(message.Text, step);
                if (isSuccessful)
                {
                    switch (step)
                    {
                        case 1:
                            _notificationService.RequestNotificationDTO.From = data[0];
                            _notificationService.RequestNotificationDTO.To = data[1];
                            break;
                        case 2:
                            _notificationService.RequestNotificationDTO.TicketTime = data[0];
                            break;
                        case 3:
                            _notificationService.RequestNotificationDTO.Days = data[0];
                            break;
                        case 4:
                            _notificationService.RequestNotificationDTO.Time = data[0];
                            break;
                        case 5:
                            _notificationService.RequestNotificationDTO.DaysToTrip = int.Parse(data[0]);
                            break;
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Помилка:\n" + data[1],
                        cancellationToken: cancellationToken);
                    return step;
                }
            }

            if (step == SD.notificationUpdate_command_steps)
            {
                ResponseModelDTO response = await _notificationService.UpdateNotification();

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: response.Message!,
                    parseMode: ParseMode.Html,
                    disableNotification: true,
                    cancellationToken: cancellationToken);

                _notificationService.RequestNotificationDTO = new();
                return 0;
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: SD.notification_command_messages[step],
                parseMode: ParseMode.Html,
                replyMarkup: ReplyKeyboards.notificationReplyMarkups[step],
                disableNotification: true,
                cancellationToken: cancellationToken);
            return ++step;
        }

        private (bool isSuccessful, string[] data) ValidateInput(string message, int commandStep)
        {
            string[] result = commandStep switch
            {
                1 => _validationService.ValidateRoute(message),
                2 => _validationService.ValidateTime(message),
                3 => _validationService.ValidateDays(message),
                4 => _validationService.ValidateTime(message),
                5 => _validationService.ValidateDaysNumber(message),
                _ => new string[2]
            };

            if (string.IsNullOrEmpty(result[0]))
                return (false, result);
            return (true, result);
        }
    }
}
