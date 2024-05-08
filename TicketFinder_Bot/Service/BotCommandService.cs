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
            string[] result;
            switch (commandStep)
            {
                case 0:
                    result = _validationService.ValidateRoute(message);
                    if (string.IsNullOrEmpty(result[0]))
                        return (false, result);
                    return (true, result);

                case 1:
                    result = _validationService.ValidateDate(message);
                    if (string.IsNullOrEmpty(result[0]))
                        return (false, result);
                    return (true, result);

                case 2:
                    result = new string[1] { message };
                    return (true, result);

                default:
                    return (false, null);
            }
        }
    }
}
