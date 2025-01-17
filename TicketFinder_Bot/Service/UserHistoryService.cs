﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Common;
using TicketFinder_Models;

namespace TicketFinder_Bot.Service
{
    public class UserHistoryService : IUserHistoryService
    {
        private readonly HttpClient _client;

        public UserHistoryService(HttpClient client)
        {
            _client = client;
        }

        public async Task<ResponseModelDTO> GetUserHistory(long chatId)
        {
            var response = await _client.GetAsync($"user-histories?chatId={chatId}");
            if (response.IsSuccessStatusCode)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UserHistoryDTO>(contentTemp);
                return new ResponseModelDTO { IsSuccess = true, Data = result };
            }
            if ((int)response.StatusCode == 404)
            {
                return new ResponseModelDTO { IsSuccess = false, Message = "Історія пуста" };
            }
            return new ResponseModelDTO { IsSuccess = false, Message = "Сталася помилка серверу" };
        }

        public async Task<ResponseModelDTO> UpdateUserHistory(UserHistoryDTO userHistoryDTO)
        {
            var content = JsonConvert.SerializeObject(userHistoryDTO);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync("user-histories", bodyContent);
            if (response.IsSuccessStatusCode)
            {
                return new ResponseModelDTO { IsSuccess = true };
            }
            return new ResponseModelDTO { IsSuccess = false };
        }
    }
}
