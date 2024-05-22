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

            Thread.Sleep(TimeSpan.FromMinutes(sleepTime));

            using CronosPeriodicTimer timer = new("0 * * * * *", CronFormat.IncludeSeconds);
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
                        if (time.Hour != DateTime.Now.Hour)
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
                            List<TicketDTO> tickets = (List<TicketDTO>)response.Data!;
                            if (tickets.Any())
                            {
                                await _botClient.SendTextMessageAsync(
                                    chatId: notification.ChatId,
                                    text: SD.ConsturctNotifyMessage(notification),
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cancellationToken);;

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
    }
}
