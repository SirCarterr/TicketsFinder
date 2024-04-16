using TicketFinder_Models;

namespace TicketsFinder_API.Services.IServices
{
    public interface ITicketsService
    {
        public List<TicketDTO> SearchTickets(string from, string to, string? date, string? time);
    }
}
