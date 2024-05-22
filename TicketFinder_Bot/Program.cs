using Microsoft.Extensions.Configuration;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Bot.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace TicketFinder_Bot
{
    public class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;
                config.AddUserSecrets<Program>(true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var config = hostContext.Configuration;
                var apiUrl = config.GetSection("API")["url"];
                var apiKey = config.GetSection("API")["key"];
                var botToken = config.GetSection("Telegram")["bot_token"]; // change for your token in user secrets

                if (apiUrl == null)
                    throw new ArgumentNullException("API url or key is not configured in user secrets.");
                if (apiKey == null)
                    throw new ArgumentNullException("API url or key is not configured in user secrets.");
                if (botToken == null)
                    throw new ArgumentNullException("API url or key is not configured in user secrets.");

                services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

                services.AddHostedService<Worker>();
                services.AddHostedService<Notifier>();

                services.AddHttpClient<ITicketService, TicketService>(options =>
                {
                    options.BaseAddress = new Uri(apiUrl);
                    options.DefaultRequestHeaders.Add("XApiKey", apiKey);
                });
                services.AddHttpClient<INotificationService, NotificationService>(options =>
                {
                    options.BaseAddress = new Uri(apiUrl);
                    options.DefaultRequestHeaders.Add("XApiKey", apiKey);
                });
                services.AddHttpClient<IUserHistoryService, UserHistoryService>(options =>
                {
                    options.BaseAddress = new Uri(apiUrl);
                    options.DefaultRequestHeaders.Add("XApiKey", apiKey);
                });

                services.AddScoped<ISearchCommandService, SearchCommandService>();
                services.AddScoped<INotificationCommandService, NotificationCommandService>();
                services.AddScoped<IHistoryCommandService, HistoryCommandService>();
                services.AddScoped<IValidationService, ValidationService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
    }
}
