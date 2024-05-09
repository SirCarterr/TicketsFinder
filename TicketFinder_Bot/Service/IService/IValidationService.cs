using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketFinder_Bot.Service.IService
{
    public interface IValidationService
    {
        public string[] ValidateRoute(string input);
        public string[] ValidateDate(string input);
        public string[] ValidateTime(string input);
    }
}
