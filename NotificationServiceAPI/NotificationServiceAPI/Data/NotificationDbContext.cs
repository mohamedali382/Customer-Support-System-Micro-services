using Microsoft.EntityFrameworkCore;
using NotificationServiceAPI.Entities;

namespace NotificationServiceAPI.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(
            DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.Property(n => n.RecipientId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(n => n.RecipientType)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(n => n.TicketTitle)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(n => n.Description)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(n => n.Type)
                    .IsRequired()
                    .HasConversion<string>();
                entity.HasIndex(n => n.RecipientId);

                entity.HasIndex(n => n.RecipientType);

                entity.HasIndex(n => n.IsRead);

                entity.HasIndex(n => n.TicketId);

                entity.HasIndex(n => n.CreatedAt);

                entity.HasIndex(n =>
                    new
                    {
                        n.RecipientId,
                        n.RecipientType,
                        n.IsRead
                    });
            });
        }
    }
}