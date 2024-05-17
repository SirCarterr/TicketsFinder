using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketFinder_Common;
using TicketFinder_Models;

namespace TicketFinder_Bot.Service.IService
{
    public class UserHistoryService : IUserHistoryService
    {
        private readonly HttpClient _client;

        public UserHistoryService()
        {
            _client = new HttpClient();
        }

        public async Task<ResponseModelDTO> GetUserHistory(int chatId)
        {
            var response = await _client.GetAsync(SD.api_url + $"user-histories?chatId={chatId}");
            if (response.IsSuccessStatusCode)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UserHistoryDTO>(contentTemp);
                return new ResponseModelDTO { IsSuccess = false, Data = result };
            }
            return new ResponseModelDTO { IsSuccess = false };
        }

        public async Task<ResponseModelDTO> UpdateUserHistory(UserHistoryDTO userHistoryDTO)
        {
            var content = JsonConvert.SerializeObject(userHistoryDTO);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync(SD.api_url + "user-histories", bodyContent);
            if (response.IsSuccessStatusCode)
            {
                return new ResponseModelDTO { IsSuccess = true};
            }
            if (response.StatusCode.Equals(404))
            {
                return new ResponseModelDTO { IsSuccess = true, Message = "Сповіщення не зайдене" };
            }
            if (response.StatusCode.Equals(502))
            {
                return new ResponseModelDTO { IsSuccess = false, Message = "Сталася помилка збереження даних" };
            }
            return new ResponseModelDTO { IsSuccess = false, Message = "Сталася помилка серверу" };
        }
    }
}
