﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketFinder_Models;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Controllers
{
    [Route("api/user-histories")]
    [ApiController]
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
                if (string.IsNullOrEmpty(userHistory_db.Id.ToString()))
                {
                    await _userHistoryService.CreateHistory(userHistoryDTO);
                }
                else
                {
                    await _userHistoryService.UpdateHistory(userHistoryDTO);
                }
                return StatusCode(StatusCodes.Status202Accepted);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error: " + ex.InnerException?.Message, DateTime.Now.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int chatId)
        {
            UserHistoryDTO userHistoryDTO = await _userHistoryService.GetHistory(chatId);
            if (string.IsNullOrEmpty(userHistoryDTO.Id.ToString()))
                return StatusCode(StatusCodes.Status404NotFound);
            return Ok(userHistoryDTO);
        }
    }
}
