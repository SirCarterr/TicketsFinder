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
        private readonly string url = "https://localhost:7196/api/tickets";
        private readonly HttpClient _client;

        public string[] RequestSearch { get; set; } //[from, to, date, time]

        public TicketService()
        {
            _client = new();
            RequestSearch = new string[4];
        }

        public async Task<List<TicketDTO>> GetTickets()
        {
            string fullUrl = url + $"?from={RequestSearch[0]}&to={RequestSearch[1]}";
            fullUrl += $"&date={RequestSearch[2]}";
            fullUrl += $"&time={RequestSearch[3]}";

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
