using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TicketFinder_Bot.Service.IService;

namespace TicketFinder_Bot.Service
{
    public class BotCommandService : IBotCommandService
    {
        private readonly IValidationService _validationService;

        public BotCommandService()
        {
            _validationService = new ValidationService();
        }

        public (bool isSuccessful, string[] data) SearchCommand(string message, int commandStep)
        {
            string[] result = commandStep switch
            {
                0 => _validationService.ValidateRoute(message),
                1 => _validationService.ValidateDate(message),
                2 => _validationService.ValidateTime(message),
                _ => new string[2]
            };

            if (string.IsNullOrEmpty(result[0]))
                return (false, result);
            return (true, result);
        }

        public (bool isSuccessful, string[] data) NotificationCreateCommand(string message, int commandStep)
        {
            string[] result = commandStep switch
            {
                0 => _validationService.ValidateRoute(message),
                1 => _validationService.ValidateTime(message),
                2 => _validationService.ValidateDays(message),
                3 => _validationService.ValidateTime(message),
                4 => _validationService.ValidateDaysNumber(message),
                _ => new string[2]
            };

            if (string.IsNullOrEmpty(result[0]))
                return (false, result);
            return (true, result);
        }
    }
}
