using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketFinder_Models
{
    public class NotificationDTO
    {
        public Guid Id { get; set; }      
        public long ChatId { get; set; }
        [Required]
        public string From { get; set; }
        [Required]
        public string To { get; set; }
        [Required]
        public string Days { get; set; }
        [Required]
        public string Time { get; set; } //in hours
    }
}
