
using AspNetCore.Authentication.ApiKey;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading.RateLimiting;
using TicketsFinder_API.Helper;
using TicketsFinder_API.Models.Data;
using TicketsFinder_API.Services;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Logging setup
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddRateLimiter(options =>
            {
                options.AddPolicy("Parser", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(httpContext.Request.Headers.Host.ToString(),
                partion => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 2,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsync(
                            $"Request limit reached. Try again in {retryAfter.TotalMinutes} minutes.", cancellationToken: token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsync(
                            "Request limit reached. Try again later", cancellationToken: token);
                    }
                };
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddSingleton<IWebDriver, ChromeDriver>(sp =>
            {
                //setup chrome options
                ChromeOptions options = new();
                options.AddArgument($"--user-agent=Mozilla/5.0 (X11; CrOS x86_64 8172.45.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.64 Safari/537.36");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--start-maximized");
                options.PageLoadStrategy = PageLoadStrategy.Normal;

                return new ChromeDriver(options);
            });
            builder.Services.AddSingleton<ITicketsService, TicketsService>();

            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IUserHistoryService, UserHistotyService>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = ApiKeyDefaults.AuthenticationScheme;
                options.DefaultScheme = ApiKeyDefaults.AuthenticationScheme;
            }).AddApiKeyInHeader<ApiKeyProvider>(ApiKeyDefaults.AuthenticationScheme, options =>
            {
                options.KeyName = "XApiKey";
                options.SuppressWWWAuthenticateHeader = true;
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TicketsFinder_API", Version = "v1" });
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "ApiKey must appear in header",
                    Type = SecuritySchemeType.ApiKey,
                    Name = "XApiKey",
                    In = ParameterLocation.Header,
                    Scheme = "ApiKeyScheme"
                });
                var key = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    },
                    In = ParameterLocation.Header
                };
                var requirement = new OpenApiSecurityRequirement
                {
                    { key, new List<string>() }
                };
                c.AddSecurityRequirement(requirement);
            });

            builder.Services.AddRouting(opt => opt.LowercaseUrls = true);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapControllers();

            var lifetime = app.Lifetime;
            var ticketsService = app.Services.GetRequiredService<ITicketsService>();
            lifetime.ApplicationStopping.Register(() =>
            {
                if (ticketsService is IDisposable disposableService)
                {
                    disposableService.Dispose();
                }
            });

            app.Run();
        }
    }
}
