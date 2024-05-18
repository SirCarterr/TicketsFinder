using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketFinder_Models
{
    public class UserHistoryDTO
    {
        public Guid Id { get; set; }
        public long ChatId { get; set; }
        [Required]
        public string History { get; set; }
    }
}
