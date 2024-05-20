using Microsoft.Extensions.Configuration;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Bot.Service;
using TicketFinder_Common;
using TicketFinder_Models;
using Telegram.Bot.Types.ReplyMarkups;
using TicketFinder_Bot.Helper;

namespace TicketFinder_Bot
{
    public class Program
    {
        private static readonly IBotCommandService _botCommandService = new BotCommandService();
        private static readonly ITicketService _ticketService = new TicketService();
        private static readonly IUserHistoryService _userHistoryService = new UserHistoryService();
        private static readonly INotificationService _notificationService = new NotificationService();

        private static string currentCommand = "";
        private static int currentCommandSteps = 0;

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            ResponseModelDTO response;

            // Callback processing
            if (update.Type == UpdateType.CallbackQuery)
            {
                CallbackQuery callbackQuery = update.CallbackQuery!;

                if (!string.IsNullOrEmpty(currentCommand))
                    await botClient.SendTextMessageAsync(
                            chatId: callbackQuery.Message!.Chat.Id,
                            text: "В даний момент виконується інша команда",
                            parseMode: ParseMode.Html,
                            disableNotification: true,
                            cancellationToken: cancellationToken);

                if (callbackQuery.Data is not { } callbackQueryData)
                    return;

                string[] data = callbackQueryData.Split(" ");
                switch (data[0])    
                {
                    case "search":
                        string[] route = data[1].Split("-");
                        currentCommand = SD.search_command;
                        _ticketService.RequestSearch[currentCommandSteps] = route[0];
                        _ticketService.RequestSearch[currentCommandSteps + 1] = route[1];
                        await _userHistoryService.UpdateUserHistory(new() { ChatId = callbackQuery.Message!.Chat.Id, History = $"{route[0]}-{route[1]}" });

                        await botClient.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: $"Виконую пошук: <b>{route[0]} -> {route[1]}</b>",
                            parseMode: ParseMode.Html,
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: SD.search_command_messages[++currentCommandSteps],
                            parseMode: ParseMode.Html,
                            replyMarkup: ReplyKeyboards.searchReplyMarkups[currentCommandSteps],
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                        break;
                    case "notification-update":
                        response = await _notificationService.GetNotifications(callbackQuery.Message!.Chat.Id);
                        if (response != null)
                        {
                            NotificationDTO? notificationDTO = ((IEnumerable<NotificationDTO>)response.Data!).FirstOrDefault(n => n.Id == Guid.Parse(data[1]));
                            if(notificationDTO != null)
                            {
                                currentCommand = SD.notificationUpdate_command;
                                _notificationService.RequestNotificationDTO = notificationDTO;

                                await botClient.SendTextMessageAsync(
                                    chatId: callbackQuery.Message.Chat.Id,
                                    text: $"Виконую редагування обраного сповіщення:\nЩоб пропустити поле введіть <b>\"-\"</b>",
                                    parseMode: ParseMode.Html,
                                    disableNotification: true,
                                    cancellationToken: cancellationToken);
                                await botClient.SendTextMessageAsync(
                                    chatId: callbackQuery.Message.Chat.Id,
                                    text: SD.notification_command_messages[currentCommandSteps],
                                    parseMode: ParseMode.Html,
                                    disableNotification: true,
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: callbackQuery.Message.Chat.Id,
                                    text: $"Це сповіщення вже не існує",
                                    parseMode: ParseMode.Html,
                                    disableNotification: true,
                                    cancellationToken: cancellationToken);
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: $"Неможливо оновити це сповіщення",
                                parseMode: ParseMode.Html,
                                disableNotification: true,
                                cancellationToken: cancellationToken);
                        }                       
                        break;
                    case "notification-delete":
                        response = await _notificationService.GetNotifications(callbackQuery.Message!.Chat.Id);
                        if (response != null)
                        {
                            NotificationDTO? notificationDTO = ((IEnumerable<NotificationDTO>)response.Data!).FirstOrDefault(n => n.Id == Guid.Parse(data[1]));
                            if (notificationDTO != null)
                            {
                                currentCommand = SD.notificationDelete_command;
                                _notificationService.RequestNotificationDTO.Id = notificationDTO.Id;

                                await botClient.SendTextMessageAsync(
                                    chatId: callbackQuery.Message.Chat.Id,
                                    text: $"Ви дійсно хочете видалити це сповіщення?",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: ReplyKeyboards.deleteReplyMarkup,
                                    disableNotification: true,
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: callbackQuery.Message.Chat.Id,
                                    text: $"Це сповіщення вже не існує",
                                    parseMode: ParseMode.Html,
                                    disableNotification: true,
                                    cancellationToken: cancellationToken);
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: $"Неможливо видалити це сповіщення",
                                parseMode: ParseMode.Html,
                                disableNotification: true,
                                cancellationToken: cancellationToken);
                        }
                        break;
                    default:
                        break;
                }
                return;
            }

            // If input is not a text message
            if (update.Message is not { } message)
                return;

            var chatId = message.Chat.Id;
            
            if (update.Message.Text is not { } messageText)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Бот приймає тільки текстові команди",
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return;
            }

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            // If no command is executed
            if (string.IsNullOrEmpty(currentCommand))
            {
                switch (messageText)
                {
                    case "/search":
                        currentCommand = SD.search_command;
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: SD.search_command_messages[currentCommandSteps],
                            parseMode: ParseMode.Html,
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                        break;
                    case "/history":
                        response = await _userHistoryService.GetUserHistory(message.Chat.Id);
                        if (response.IsSuccess)
                        {
                            UserHistoryDTO userHistoryDTO = (UserHistoryDTO)response.Data!;
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Історія останніх 5 пошуків:",
                                replyMarkup: ReplyKeyboards.GetUserHistoryMarkup(userHistoryDTO),
                                disableNotification: true,
                                cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: response.Message!,
                                disableNotification: true,
                                cancellationToken: cancellationToken);
                        }
                        break;
                    case "/notifications":
                        response = await _notificationService.GetNotifications(chatId);
                        if (response.IsSuccess)
                        {
                            IEnumerable<NotificationDTO> notificationDTOs = (IEnumerable<NotificationDTO>)response.Data!;
                            if (notificationDTOs.Any())
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: SD.notifications_any,
                                    disableNotification: true,
                                    cancellationToken: cancellationToken);
                                foreach (NotificationDTO notificationDTO in notificationDTOs)
                                {
                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: SD.ConstructNotificationMessage(notificationDTO),
                                        parseMode: ParseMode.Html,
                                        replyMarkup: ReplyKeyboards.GetNotificationMarkup(notificationDTO),
                                        disableNotification: true,
                                        cancellationToken: cancellationToken);
                                }
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: SD.notifications_empty,
                                    disableNotification: true,
                                    cancellationToken: cancellationToken);
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: response.Message!,
                                disableNotification: true,
                                cancellationToken: cancellationToken);
                        }
                        break;
                    case "/notification-create":
                        currentCommand = SD.notificationCreate_command;
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: SD.notification_command_messages[currentCommandSteps],
                            parseMode: ParseMode.Html,
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                        break;
                    case "/cancel":
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Жодна команда не виконується",
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                        break;
                    default:
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Такої команди не існує",
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                        break;
                }
                return;
            }

            // Сancel executed command
            if (messageText.Equals("/cancel"))
            {
                currentCommand = "";
                currentCommandSteps = 0;

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Команду відмінено",
                    replyMarkup: new ReplyKeyboardRemove(),
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return;
            }

            // Check what command is executed
            (bool isSuccessful, string[] data) commandResult;
            switch (currentCommand)
            {
                case "/search":
                    commandResult = _botCommandService.SearchCommand(messageText, currentCommandSteps);
                    if (commandResult.isSuccessful)
                    {
                        if (currentCommandSteps == 0)
                        {
                            _ticketService.RequestSearch[currentCommandSteps] = commandResult.data[0];
                            _ticketService.RequestSearch[currentCommandSteps + 1] = commandResult.data[1];
                            await _userHistoryService.UpdateUserHistory(new() { ChatId = chatId, History = $"{commandResult.data[0]}-{commandResult.data[1]}" });
                        }
                        else
                        {
                            _ticketService.RequestSearch[currentCommandSteps + 1] = commandResult.data[0];
                        }

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: SD.search_command_messages[++currentCommandSteps],
                            parseMode: ParseMode.Html,
                            replyMarkup: ReplyKeyboards.searchReplyMarkups[currentCommandSteps],
                            disableNotification: true,
                            cancellationToken: cancellationToken);

                        if (currentCommandSteps == SD.search_command_steps)
                        {
                            List<TicketDTO> tickets = await _ticketService.GetTickets();
                            if (!tickets.Any())
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Немає квитків на вказаний маршрут за введеними даними",
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Знайдені квитки на вказаний маршрут:",
                                    cancellationToken: cancellationToken);

                                foreach (TicketDTO ticket in tickets)
                                {
                                    InlineKeyboardButton[] inlineKeyboardButtons = new InlineKeyboardButton[ticket.Items.Count];
                                    for(int i = 0; i < ticket.Items.Count; i++)
                                    {
                                        inlineKeyboardButtons[i] = InlineKeyboardButton.WithUrl($"{ticket.Items[i].Class}: {ticket.Items[i].Places}", ticket.Items[i].URL);
                                    }

                                    InlineKeyboardMarkup inlineKeyboardMarkup = new(inlineKeyboardButtons);                 

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: SD.ConstructTicketMessage(ticket),
                                        parseMode: ParseMode.Html,
                                        replyMarkup: inlineKeyboardMarkup,
                                        cancellationToken: cancellationToken);
                                } 
                            }
                            currentCommand = "";
                            currentCommandSteps = 0;
                            _ticketService.RequestSearch = new string[4];
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Помилка:\n" + commandResult.data[1],
                            cancellationToken: cancellationToken);
                    }
                    break;
                case "/notification-create":
                    commandResult = _botCommandService.NotificationCreateCommand(messageText, currentCommandSteps);
                    if (commandResult.isSuccessful)
                    {
                        switch (currentCommandSteps)
                        {
                            case 0:
                                _notificationService.RequestNotificationDTO.From = commandResult.data[0];
                                _notificationService.RequestNotificationDTO.To = commandResult.data[1];
                                break;
                            case 1:
                                _notificationService.RequestNotificationDTO.TicketTime = commandResult.data[0];
                                break;
                            case 2:
                                _notificationService.RequestNotificationDTO.Days = commandResult.data[0];
                                break;
                            case 3:
                                _notificationService.RequestNotificationDTO.Time = commandResult.data[0];
                                break;
                            case 4:
                                _notificationService.RequestNotificationDTO.DaysToTrip = int.Parse(commandResult.data[0]);
                                break;
                        }

                        if (currentCommandSteps == SD.notificationCreate_command_steps)
                        {
                            _notificationService.RequestNotificationDTO.ChatId = chatId;
                            response = await _notificationService.CreateNotification();

                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: response.Message!,
                                parseMode: ParseMode.Html,
                                disableNotification: true,
                                cancellationToken: cancellationToken);

                            currentCommand = "";
                            currentCommandSteps = 0;
                            _notificationService.RequestNotificationDTO = new();
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: SD.notification_command_messages[++currentCommandSteps],
                                parseMode: ParseMode.Html,
                                replyMarkup: ReplyKeyboards.notificationReplyMarkups[currentCommandSteps],
                                disableNotification: true,
                                cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Помилка:\n" + commandResult.data[1],
                        cancellationToken: cancellationToken);
                    }
                    break;
                case "/notification-update":
                    if (!messageText.Trim().Equals("-"))
                    {
                        commandResult = _botCommandService.NotificationCreateCommand(messageText, currentCommandSteps);
                        if (commandResult.isSuccessful)
                        {
                            switch (currentCommandSteps)
                            {
                                case 0:
                                    _notificationService.RequestNotificationDTO.From = commandResult.data[0];
                                    _notificationService.RequestNotificationDTO.To = commandResult.data[1];
                                    break;
                                case 1:
                                    _notificationService.RequestNotificationDTO.TicketTime = commandResult.data[0];
                                    break;
                                case 2:
                                    _notificationService.RequestNotificationDTO.Days = commandResult.data[0];
                                    break;
                                case 3:
                                    _notificationService.RequestNotificationDTO.Time = commandResult.data[0];
                                    break;
                                case 4:
                                    _notificationService.RequestNotificationDTO.DaysToTrip = int.Parse(commandResult.data[0]);
                                    break;
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Помилка:\n" + commandResult.data[1],
                                cancellationToken: cancellationToken);
                            return;
                        }
                    }

                    if (currentCommandSteps == SD.notificationCreate_command_steps)
                    {
                        response = await _notificationService.UpdateNotification();

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: response.Message!,
                            parseMode: ParseMode.Html,
                            disableNotification: true,
                            cancellationToken: cancellationToken);

                        currentCommand = "";
                        currentCommandSteps = 0;
                        _notificationService.RequestNotificationDTO = new();
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: SD.notification_command_messages[++currentCommandSteps],
                            parseMode: ParseMode.Html,
                            replyMarkup: ReplyKeyboards.notificationReplyMarkups[currentCommandSteps],
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                    }
                    break;
                case "/notification-delete":
                    if (messageText.Trim().ToLower().Equals("так"))
                    {
                        response = await _notificationService.DeleteNotification();

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: response.Message!,
                            parseMode: ParseMode.Html,
                            replyMarkup: new ReplyKeyboardRemove(),
                            disableNotification: true,
                            cancellationToken: cancellationToken);

                        currentCommand = "";
                        _notificationService.RequestNotificationDTO = new();
                    }
                    else if (messageText.Trim().ToLower().Equals("ні"))
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Команда відмінена",
                            parseMode: ParseMode.Html,
                            replyMarkup: new ReplyKeyboardRemove(),
                            disableNotification: true,
                            cancellationToken: cancellationToken);

                        currentCommand = "";
                        _notificationService.RequestNotificationDTO = new();
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Невірний формат вводу, спробуйте ще",
                            parseMode: ParseMode.Html,
                            replyMarkup: new ReplyKeyboardRemove(),
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                    }
                    break;
            }
            return;
        }

        public static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        static void Main(string[] args)
        {
            ConfigurationBuilder configBuilder = new();
            IConfiguration config = configBuilder.AddUserSecrets<Program>().Build();
            string token = config.GetSection("Telegram")["bot_token"]!;

            var botClient = new TelegramBotClient(token);

            using CancellationTokenSource cts = new();

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() 
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            while (true)
            {
                if (Console.ReadLine() == "stop")
                {
                    cts.Cancel();
                    break;
                }
            }  
        }
    }
}
