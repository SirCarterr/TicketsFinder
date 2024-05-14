using Microsoft.EntityFrameworkCore;

namespace TicketsFinder_API.Models.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserHistory> UserHistories { get; set; }
    }
}
