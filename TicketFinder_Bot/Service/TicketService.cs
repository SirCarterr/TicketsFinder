using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Models;

namespace TicketFinder_Bot.Service
{
    public class TicketService : ITicketService
    {
        private readonly string url = "https://localhost:7196/tickets";
        private readonly HttpClient _client;

        public TicketService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<TicketDTO>> GetTicket(string from, string to, string? date, string? time)
        {
            string fullUrl = url + $"?from={from}&to={to}";
            fullUrl += date != null ? $"&date={date}" : "";
            fullUrl += time != null ? $"&time={time}" : "";

            var response = await _client.GetAsync(fullUrl);
            if (response.IsSuccessStatusCode)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<TicketDTO>>(contentTemp);
                return result!;
            }
            return new();
        }
    }
}
