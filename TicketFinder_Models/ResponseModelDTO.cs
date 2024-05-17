using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketFinder_Models
{
    public class ResponseModelDTO
    {
        public bool IsSuccess { get; set; }
        public object? Data { get; set; }
        public string? Message { get; set; }
    }
}
