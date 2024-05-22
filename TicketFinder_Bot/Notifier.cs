using Cronos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TicketFinder_Bot.Helper;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Common;
using TicketFinder_Models;

namespace TicketFinder_Bot
{
    public class Notifier : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly INotificationService _notificationService;
        private readonly ITicketService _ticketService;

        private readonly Dictionary<string, DayOfWeek> daysValue = new()
        {
            { "понеділок", DayOfWeek.Monday },
            { "вівторок", DayOfWeek.Tuesday },
            { "середа", DayOfWeek.Wednesday },
            { "четвер", DayOfWeek.Thursday },
            { "п'ятниця", DayOfWeek.Friday },
            { "субота", DayOfWeek.Saturday },
            { "неділя", DayOfWeek.Sunday },
        };

        private readonly Dictionary<string, List<DayOfWeek>> shortDaysValue = new()
        {
            { "парні", new() { DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Saturday } },
            { "непарні", new() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Sunday } },
            { "будні", new() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday } },
            { "вихідні", new() { DayOfWeek.Saturday, DayOfWeek.Sunday } },
        };

        public Notifier(ILogger<Notifier> logger, IConfiguration configuration, 
            ITelegramBotClient botClient, INotificationService notificationService, ITicketService ticketService)
        {
            _logger = logger;
            _botClient = botClient;
            _notificationService = notificationService;
            _ticketService = ticketService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(Notifier)} started");

            int sleepTime = 60 - DateTime.Now.Minute;
            _logger.LogInformation($"Waiting for {sleepTime} minute(s) till first tick");

            using CronosPeriodicTimer timer = new("0 * * * * ", CronFormat.Standard);
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                _logger.LogInformation($"Start notifying at {DateTime.Now.ToShortTimeString()}");

                ResponseModelDTO response = await _notificationService.GetNotifications(null);
                if (response.IsSuccess) 
                {
                    IEnumerable<NotificationDTO> notifications = (IEnumerable<NotificationDTO>)response.Data!;
                    foreach (var notification in notifications)
                    {
                        var time = TimeOnly.ParseExact(notification.Time, "HH:mm", null, DateTimeStyles.None);
                        if (!IsInCurrentDayRange(notification.Days) || time.Hour != DateTime.Now.Hour)
                            continue;

                        string[] search = {
                            notification.From,
                            notification.To,
                            DateTime.Now.AddDays(notification.DaysToTrip).ToString("dd.MM.yyyy"),
                            notification.TicketTime
                        };

                        response = await _ticketService.GetTickets(search);
                        if (response.IsSuccess)
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: notification.ChatId,
                                text: SD.ConsturctNotifyMessage(notification),
                                parseMode: ParseMode.Html,
                                cancellationToken: cancellationToken);
                            List<TicketDTO> tickets = (List<TicketDTO>)response.Data!;
                            if (tickets.Any())
                            {
                                foreach (TicketDTO ticket in tickets)
                                {
                                    await _botClient.SendTextMessageAsync(
                                        chatId: notification.ChatId,
                                        text: SD.ConstructTicketMessage(ticket),
                                        parseMode: ParseMode.Html,
                                        replyMarkup: ReplyKeyboards.GetTicketReplyMarkup(ticket),
                                        cancellationToken: cancellationToken);
                                }
                            }
                            else
                            {
                                await _botClient.SendTextMessageAsync(
                                    chatId: notification.ChatId,
                                    text: "Зараз немає квитків на вказану дату та час",
                                    cancellationToken: cancellationToken);
                            }
                        }
                    }
                }
            }
        }

        private bool IsInCurrentDayRange(string days)
        {
            DayOfWeek currentDay = DateTime.Now.DayOfWeek;

            // Check for special days value
            List<DayOfWeek> specifiedDays = shortDaysValue.TryGetValue(days, out List<DayOfWeek>? value) ? value : new();

            if (!specifiedDays.Any())
            {
                // Convert the string days to DayOfWeek enum values
                string[] daysArray = days.Split(", ");
                foreach (string day in daysArray)
                {
                    if (daysValue.TryGetValue(day.ToLowerInvariant(), out DayOfWeek dayOfWeek))
                    {
                        specifiedDays.Add(dayOfWeek);
                    }
                }
            }

            // Check if the current day of the week is in the specified days
            return specifiedDays.Contains(currentDay);
        }
    }
}
