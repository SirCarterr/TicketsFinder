﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketFinder_Models;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Controllers
{
    [Route("api/user-histories")]
    [ApiController]
    [Authorize]
    public class UserHistoriesController : ControllerBase
    {
        private readonly IUserHistoryService _userHistoryService;
        private readonly ILogger _logger;

        public UserHistoriesController(IUserHistoryService userHistoryService, ILogger<UserHistoriesController> logger)
        {
            _userHistoryService = userHistoryService;
            _logger = logger;
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserHistoryDTO userHistoryDTO)
        {
            try
            {
                UserHistoryDTO userHistory_db = await _userHistoryService.GetHistory(userHistoryDTO.ChatId);
                if (userHistory_db.Id == Guid.Empty)
                {
                    _logger.LogInformation($"UserHistory created for chat {userHistoryDTO.ChatId}");
                    await _userHistoryService.CreateHistory(userHistoryDTO);
                }
                else
                {
                    _logger.LogInformation($"UserHistory updated for chat {userHistoryDTO.ChatId}");
                    await _userHistoryService.UpdateHistory(userHistoryDTO);
                }
                return StatusCode(StatusCodes.Status202Accepted);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error: " + ex.InnerException?.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] long chatId)
        {
            _logger.LogInformation($"UserHistory is retrieved for chat {chatId}");
            UserHistoryDTO userHistoryDTO = await _userHistoryService.GetHistory(chatId);
            if (userHistoryDTO.Id == Guid.Empty)
            {
                _logger.LogInformation($"UserHistory not found for chat {chatId}");
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return Ok(userHistoryDTO);
        }
    }
}
