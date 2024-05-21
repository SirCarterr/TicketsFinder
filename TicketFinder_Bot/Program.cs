using Microsoft.Extensions.Configuration;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Bot.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                services.AddHostedService<Worker>();
                services.AddHttpClient();

                services.AddScoped<ISearchCommandService, SearchCommandService>();
                services.AddScoped<INotificationCommandService, NotificationCommandService>();
                services.AddScoped<IHistoryCommandService, HistoryCommandService>();

                services.AddScoped<ITicketService, TicketService>();
                services.AddScoped<IUserHistoryService, UserHistoryService>();
                services.AddScoped<INotificationService, NotificationService>();
                services.AddScoped<IValidationService, ValidationService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
    }
}
