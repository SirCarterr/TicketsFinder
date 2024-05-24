using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TicketFinder_Models;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketsService _ticketService;
        private readonly ILogger _logger;

        public TicketsController(ITicketsService ticketsService, ILogger<TicketsController> logger)
        {
            _ticketService = ticketsService;
            _logger = logger;
        }

        [HttpGet]
        [EnableRateLimiting("Parser")]
        public IActionResult SearchTickets([FromQuery] string from, [FromQuery] string to,
            [FromQuery] string? date, [FromQuery] string? time)
        {
            for (int i = 0; i < 3; i++)
            {
                ResponseModelDTO response = _ticketService.SearchTickets(from, to, date, time);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("Tickets search successful");
                    return Ok(response.Data);
                }
                if (response.Message!.Equals("site error"))
                {
                    _logger.LogError("Tickets search failed. Ticket site error");
                    return StatusCode(StatusCodes.Status502BadGateway);
                }
                if (response.Message!.Equals("unexpected error"))
                {
                    _logger.LogError("Tickets search failed. Driver error");
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            _logger.LogError("Tickets search failed. Search Timeout");
            return StatusCode(StatusCodes.Status504GatewayTimeout);
        }
    }
}
