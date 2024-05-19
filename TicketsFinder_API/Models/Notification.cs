using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketsFinder_API.Models
{
    public class Notification
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public long ChatId { get; set; }
        [Required]
        public string From { get; set; }
        [Required]
        public string To { get; set; }
        [Required]
        public string Days { get; set; } // days of week
        [Required]
        public string Time { get; set; } // in hours
        [Required]
        public int DaysToTrip { get; set; }
        [Required]
        public string TicketTime { get; set; } // in hours
    }
}
