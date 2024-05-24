using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Common;

namespace TicketFinder_Bot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITelegramBotClient _botClient;

        private readonly ISearchCommandService _searchCommandService;
        private readonly IHistoryCommandService _historyCommandService;
        private readonly INotificationCommandService _notificationCommandService;

        private string currentCommand = "";
        private int currentCommandSteps = 0;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, ITelegramBotClient botClient,
            ISearchCommandService searchCommandService, IHistoryCommandService historyCommandService, INotificationCommandService notificationCommandService)
        {
            _logger = logger;
            _botClient = botClient;
            _searchCommandService = searchCommandService;
            _historyCommandService = historyCommandService;
            _notificationCommandService = notificationCommandService;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var me = await _botClient.GetMeAsync();
            _logger.LogInformation($"Bot started: @{me.Username}");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery } // Receive specified update types
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandleMessageAsync(botClient, update.Message, cancellationToken);
            }
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data != null)
            {
                await HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
            }
        }

        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received message \"{message.Text}\" in chat {message.Chat.Id}");

            // If no command is executed
            if (string.IsNullOrEmpty(currentCommand))
            {
                switch (message.Text)
                {
                    case "/start":
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: SD.start_command,
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                        break;
                    case "/search":
                        currentCommand = SD.search_command;
                        currentCommandSteps = await _searchCommandService.SearchCommand(botClient, message, currentCommandSteps, cancellationToken);
                        break;
                    case "/history":
                        await _historyCommandService.HistoryCommand(botClient, message, cancellationToken);
                        break;
                    case "/notifications":
                        await _notificationCommandService.GetNotificationsCommand(botClient, message, cancellationToken);
                        break;
                    case "/notificationCreate":
                        currentCommand = SD.notificationCreate_command;
                        currentCommandSteps = await _notificationCommandService.CreateNotificationCommand(botClient, message, currentCommandSteps, cancellationToken);
                        break;
                    case "/cancel":
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Жодна команда не виконується",
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                        break;
                    default:
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Такої команди не існує",
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                        break;
                }
                return;
            }

            // Сancel executed command
            if (message.Text!.Equals("/cancel"))
            {
                currentCommand = "";
                currentCommandSteps = 0;

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
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
                    currentCommandSteps = await _searchCommandService.SearchCommand(botClient, message, currentCommandSteps, cancellationToken);
                    if (currentCommandSteps == 0)
                        currentCommand = "";
                    break;
                case "/notificationCreate":
                    currentCommandSteps = await _notificationCommandService.CreateNotificationCommand(botClient, message, currentCommandSteps, cancellationToken);
                    if (currentCommandSteps == 0)
                        currentCommand = "";
                    break;
                case "/notificationUpdate":
                    currentCommandSteps = await _notificationCommandService.UpdateNotificationCommand(botClient, message, currentCommandSteps, cancellationToken);
                    if (currentCommandSteps == 0)
                        currentCommand = "";
                    break;
                case "/notificationDelete":
                    currentCommandSteps = await _notificationCommandService.DeleteNotificationCommand(botClient, message, cancellationToken);
                    if (currentCommandSteps == 0)
                        currentCommand = "";
                    break;
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(currentCommand))
            {
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message!.Chat.Id,
                    text: "В даний момент виконується інша команда",
                    parseMode: ParseMode.Html,
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return;
            }

            string callBackCommand = callbackQuery.Data!.Split(" ")[0];
            _logger.LogInformation($"Received callbackQuery \"{callBackCommand}\" in chat {callbackQuery.Message!.Chat.Id}");

            switch (callBackCommand)
            {
                case "search":
                    currentCommand = SD.search_command;
                    currentCommandSteps = await _searchCommandService.SearchCallback(botClient, callbackQuery, cancellationToken);
                    break;
                case "notification-update":
                    currentCommand = SD.notificationUpdate_command;
                    currentCommandSteps = await _notificationCommandService.UpdateNotificationCallback(botClient, callbackQuery, cancellationToken);
                    break;
                case "notification-delete":
                    currentCommand = SD.notificationDelete_command;
                    currentCommandSteps = await _notificationCommandService.DeleteNotificationCallback(botClient, callbackQuery, cancellationToken);
                    break;
                default:
                    break;
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
