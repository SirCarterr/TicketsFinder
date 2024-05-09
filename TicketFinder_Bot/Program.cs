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

namespace TicketFinder_Bot
{
    public class Program
    {
        private static readonly IBotCommandService _botCommandService = new BotCommandService();
        private static readonly ITicketService _ticketService = new TicketService();

        private static string currentCommand = "";
        private static int currentCommandSteps = 0;

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

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
            switch (currentCommand)
            {
                case "/search":
                    var (isSuccessful, data) = _botCommandService.SearchCommand(messageText, currentCommandSteps);
                    if (isSuccessful)
                    {
                        if (currentCommandSteps == 0)
                        {
                            _ticketService.RequestSearch[currentCommandSteps] = data[0];
                            _ticketService.RequestSearch[currentCommandSteps + 1] = data[1];
                        }
                        else
                        {
                            _ticketService.RequestSearch[currentCommandSteps + 1] = data[0];
                        }

                        //TO DO: reply markup
                        ReplyKeyboardMarkup replyKeyboardMarkup;
                        switch (currentCommandSteps + 1)
                        {
                            case 1:
                                replyKeyboardMarkup = new(new[]
                                {
                                    new KeyboardButton[] {"Сьогодні"},
                                    new KeyboardButton[] {"Завтра"},
                                    new KeyboardButton[] {"Післязавтра"},
                                });

                                await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: SD.search_command_messages[++currentCommandSteps],
                                parseMode: ParseMode.Html,
                                replyMarkup: replyKeyboardMarkup,
                                disableNotification: true,
                                cancellationToken: cancellationToken);
                                break;
                            case 2:
                                replyKeyboardMarkup = new(new[]
                                {
                                    new KeyboardButton[] {"00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00"},
                                    new KeyboardButton[] {"08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00"},
                                    new KeyboardButton[] {"16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00"},
                                });

                                await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: SD.search_command_messages[++currentCommandSteps],
                                parseMode: ParseMode.Html,
                                replyMarkup: replyKeyboardMarkup,
                                disableNotification: true,
                                cancellationToken: cancellationToken);
                                break;
                            case 3:
                                await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: SD.search_command_messages[++currentCommandSteps],
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove(),
                                disableNotification: true,
                                cancellationToken: cancellationToken);
                                break;
                            default:
                                await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: SD.search_command_messages[++currentCommandSteps],
                                parseMode: ParseMode.Html,
                                disableNotification: true,
                                cancellationToken: cancellationToken);
                                break;
                        }

                        if (currentCommandSteps == SD.search_command_steps)
                        {
                            List<TicketDTO> tickets = await _ticketService.GetTickets();
                            if (!tickets.Any())
                            {
                                await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Немає квитків на вказаний маршрут за введеними даними(",
                                cancellationToken: cancellationToken);
                            }
                            else
                            {
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
                                    text: "Знайдені квитки на вказаний маршрут:",
                                    cancellationToken: cancellationToken);

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
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Помилка:\n" + data[1],
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
