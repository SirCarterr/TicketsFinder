using Newtonsoft.Json;
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
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _client;

        public NotificationService()
        {
            _client = new HttpClient();
        }

        public async Task<ResponseModelDTO> CreateNotification(NotificationDTO notificationDTO)
        {
            var content = JsonConvert.SerializeObject(notificationDTO);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(SD.api_url + "notifications", bodyContent);
            if (response.IsSuccessStatusCode)
            {
                return new ResponseModelDTO { IsSuccess = true, Message = "Сповіщення створене" };
            }
            if ((int)response.StatusCode == 405)
            {
                return new ResponseModelDTO { IsSuccess = false, Message = "Ви досягли ліміту в 3 сповіщення" };
            }
            if ((int)response.StatusCode == 502)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<string>(contentTemp);
                return new ResponseModelDTO { IsSuccess = false, Message = result };
            }
            return new ResponseModelDTO { IsSuccess = false, Message = "Сталася помилка серверу" };
        }

        public async Task<ResponseModelDTO> DeleteNotification(Guid id)
        {
            var response = await _client.DeleteAsync(SD.api_url + $"notifications?id={id}");
            if (response.IsSuccessStatusCode)
            {
                return new ResponseModelDTO { IsSuccess = true, Message = "Сповіщення видалене" };
            }
            if ((int)response.StatusCode == 404)
            {
                return new ResponseModelDTO { IsSuccess = false, Message = "Сповіщення не зайдене" };
            }
            if ((int)response.StatusCode == 502)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<string>(contentTemp);
                return new ResponseModelDTO { IsSuccess = false, Message = result };
            }
            return new ResponseModelDTO { IsSuccess = false, Message = "Сталася помилка серверу" };
        }

        public async Task<ResponseModelDTO> GetNotifications(int chatId)
        {
            var response = await _client.GetAsync(SD.api_url + $"notifications?chatId={chatId}");
            if (response.IsSuccessStatusCode)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<NotificationDTO>>(contentTemp);
                return new ResponseModelDTO { IsSuccess = true, Data = result };
            }
            return new ResponseModelDTO { IsSuccess = false, Message = "Сталася помилка серверу" };
        }

        public async Task<ResponseModelDTO> UpdateNotification(NotificationDTO notificationDTO)
        {
            var content = JsonConvert.SerializeObject(notificationDTO);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync(SD.api_url + "notifications", bodyContent);
            if (response.IsSuccessStatusCode)
            {
                return new ResponseModelDTO { IsSuccess = true, Message = "Сповіщення оновлене" };
            }
            if ((int)response.StatusCode == 404)
            {
                return new ResponseModelDTO { IsSuccess = false, Message = "Сповіщення не зайдене" };
            }
            if ((int)response.StatusCode == 502)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<string>(contentTemp);
                return new ResponseModelDTO { IsSuccess = false, Message = result };
            }
            return new ResponseModelDTO { IsSuccess = false, Message = "Сталася помилка серверу" };
        }
    }
}
