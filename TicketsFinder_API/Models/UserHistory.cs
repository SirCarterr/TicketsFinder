using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketsFinder_API.Models
{
    public class UserHistory
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public int ChatId { get; set; }
        [Required]
        public string History { get; set; }
    }
}
