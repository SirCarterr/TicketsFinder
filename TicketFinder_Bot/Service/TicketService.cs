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
    public class TicketService : ITicketService
    {
        private readonly HttpClient _client;

        public TicketService(HttpClient client)
        {
            _client = client;
        }

        public async Task<ResponseModelDTO> GetTickets(string[] search)
        {
            string fullUrl = $"tickets?from={search[0]}&to={search[1]}";
            fullUrl += $"&date={search[2]}";
            fullUrl += $"&time={search[3]}";

            var response = await _client.GetAsync(fullUrl);
            if (response.IsSuccessStatusCode)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<TicketDTO>>(contentTemp);
                return new() { IsSuccess = true, Data = result };
            }
            if ((int)response.StatusCode == 429)
                return new() { IsSuccess = false, Message = "Сервіс перенавантажений, спробуйте ще раз через пару хвилин" };
            if ((int)response.StatusCode == 502)
                return new() { IsSuccess = false, Message = "Неможливо зробити пошук, спробуйте ще раз пізніше" };
            if ((int)response.StatusCode == 504)
                return new() { IsSuccess = false, Message = "Час запиту вийшов, спробуйте ще раз через пару хвилин" };
            return new() { IsSuccess = false, Message = "Сталась помилка при пошуку квитків" };
        }
    }
}
