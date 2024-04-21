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

        public (bool, object?) SearchCommand(string message, int commandStep)
        {
            string[] result;
            switch (commandStep)
            {
                case 1:
                    result = _validationService.ValidateRoute(message);
                    if (string.IsNullOrEmpty(result[1]))
                        return (false, null);
                    return (true, result);

                case 2:
                    result = _validationService.ValidateDate(message);
                    if (string.IsNullOrEmpty(result[0]))
                        return (false, null);
                    return (true, result);

                case 3:
                    return (true, message);

                default:
                    return (false, null);
            }
        }
    }
}
