using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models;

namespace HelpDesk.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Feedback> Feedbacks { get; set; }

        // Kişi 2 tarafından doldurulacak:
        // public DbSet<Ticket> Tickets { get; set; }
        // public DbSet<TicketReply> TicketReplies { get; set; }
        // public DbSet<Category> Categories { get; set; }
    }
}
