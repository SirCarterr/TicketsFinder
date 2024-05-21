using TicketFinder_Models;

namespace TicketsFinder_API.Services.IServices
{
    public interface ITicketsService
    {
        public ResponseModelDTO SearchTickets(string from, string to, string? date, string? time);
    }
}
