using Microsoft.Extensions.Configuration;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Bot.Service;
using TicketFinder_Common;

namespace TicketFinder_Bot
{
    public class Program
    {
        private static readonly IValidationService _validationService = new ValidationService();
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
                        currentCommandSteps++;
                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: SD.getRoute_message,
                        cancellationToken: cancellationToken);
                        break;

                    default:
                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Такої команди не існує",
                        cancellationToken: cancellationToken);
                        break;
                }
            }

            // Check what command is executed
            switch (currentCommand)
            {
                case "/search":
                    //Use IBotService to do command steps
                    //Then use TicketService to get tickets

                    break;
            }
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
