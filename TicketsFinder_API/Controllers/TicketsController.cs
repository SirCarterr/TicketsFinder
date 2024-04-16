using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketFinder_Models;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketsService _ticketService;

        public TicketsController(ITicketsService ticketsService)
        {
            _ticketService = ticketsService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchTickets([FromQuery] string from, [FromQuery] string to,
            [FromQuery] string? date, [FromQuery] string? time)
        {
            List<TicketDTO> ticketDTOs = _ticketService.SearchTickets(from, to, date, time);
            return Ok(ticketDTOs);
        }
    }
}
