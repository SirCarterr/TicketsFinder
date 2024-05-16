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

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationDTO notificationDTO)
        {
            try
            {
                int check = await _notificationService.CheckCount(notificationDTO.ChatId);
                if (check == 1)
                {
                    int result = await _notificationService.CreateNotification(notificationDTO);
                    return result == 1 ? StatusCode(StatusCodes.Status201Created) : StatusCode(StatusCodes.Status500InternalServerError);
                }
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, "db update error: " + ex.InnerException?.Message);
            }
            catch 
            {
                throw;
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] NotificationDTO notificationDTO)
        {
            try
            {
                int result = await _notificationService.UpdateNotification(notificationDTO);
                return result == 1 ? StatusCode(StatusCodes.Status202Accepted) : StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, "db update error: " + ex.InnerException?.Message);
            }
            catch
            {
                throw;
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
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, "db update error: " + ex.InnerException?.Message);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int chatId)
        {
            return Ok(await _notificationService.GetNotifications(chatId));
        }
    }
}
