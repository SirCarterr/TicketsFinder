using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketFinder_Models;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
                    _logger.LogInformation($"New notification created for chat {notificationDTO.ChatId}", DateTime.Now);  
                    await _notificationService.CreateNotification(notificationDTO);
                    return StatusCode(StatusCodes.Status201Created);
                }
                _logger.LogInformation($"New notification not created due to reached limit for chat {notificationDTO.ChatId}", DateTime.Now);
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
                if (result == 1)
                {
                    _logger.LogInformation($"Notification {notificationDTO.Id} updated for chat {notificationDTO.ChatId}", DateTime.Now);
                    return StatusCode(StatusCodes.Status202Accepted);
                }
                _logger.LogInformation($"Notification {notificationDTO.Id} is not found for chat {notificationDTO.ChatId}", DateTime.Now);
                return StatusCode(StatusCodes.Status404NotFound);
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
                if (result == 1)
                {
                    _logger.LogInformation($"Notification {id} deleted", DateTime.Now);
                    return Ok();
                }
                _logger.LogInformation($"Notification {id} is not found", DateTime.Now);
                return StatusCode(StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error: " + ex.InnerException?.Message, DateTime.Now.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] long chatId)
        {
            _logger.LogInformation($"Notifications for chat {chatId} are retrieved", DateTime.Now);
            return Ok(await _notificationService.GetNotifications(chatId));
        }
    }
}
