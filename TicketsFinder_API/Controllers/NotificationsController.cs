using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketFinder_Models;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;

        public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationDTO notificationDTO)
        {
            try
            {
                int check = await _notificationService.CheckCount(notificationDTO.ChatId);
                if (check == 1)
                {
                    await _notificationService.CreateNotification(notificationDTO);
                    return StatusCode(StatusCodes.Status201Created);
                }
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error: " + ex.InnerException?.Message, DateTime.Now.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] NotificationDTO notificationDTO)
        {
            try
            {
                int result = await _notificationService.UpdateNotification(notificationDTO);
                return result == 1 ? StatusCode(StatusCodes.Status202Accepted) : StatusCode(StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error: " + ex.InnerException?.Message, DateTime.Now.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] Guid id)
        {
            try
            {
                int result = await _notificationService.DeleteNotification(id);
                return result == 1 ? StatusCode(StatusCodes.Status201Created) : StatusCode(StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error: " + ex.InnerException?.Message, DateTime.Now.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int chatId)
        {
            return Ok(await _notificationService.GetNotifications(chatId));
        }
    }
}
