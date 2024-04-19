using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketFinder_Models;

namespace TicketFinder_Bot.Service.IService
{
    public interface ITicketService
    {
        public Task<List<TicketDTO>> GetTicket(string from, string to, string? date, string? time);
    }
}
