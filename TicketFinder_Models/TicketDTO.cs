using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketFinder_Models
{
    public class TicketDTO
    {
        public string Num { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public TimeOnly Duration { get; set; }
        public List<Item> Items { get; set; }

        public class Item
        {
            public string Class { get; set; }
            public int Places { get; set; }
            public string URL { get; set; }
        }
    }
}
