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
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketReply> TicketReplies { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Ticket>()
                .HasOne(t => t.Musteri)
                .WithMany()
                .HasForeignKey(t => t.MusteriId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Ticket>()
                .HasOne(t => t.AtananAjan)
                .WithMany()
                .HasForeignKey(t => t.AtananAjanId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<TicketReply>()
                .HasOne(r => r.Yazar)
                .WithMany()
                .HasForeignKey(r => r.YazarId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
