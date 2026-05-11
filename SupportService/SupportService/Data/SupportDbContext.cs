using Microsoft.EntityFrameworkCore;
using SupportService.Entities;

namespace SupportService.Data;

public class SupportDbContext : DbContext
{
    public SupportDbContext(DbContextOptions<SupportDbContext> options)
        : base(options) { }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<TicketAssignment> Assignments => Set<TicketAssignment>();
    public DbSet<TicketResponse> TicketResponses => Set<TicketResponse>();
    public DbSet<TicketSnapshot> TicketSnapshots => Set<TicketSnapshot>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id)
                  .HasMaxLength(36)
                  .ValueGeneratedNever();

            entity.Property(a => a.Name).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Email).IsRequired().HasMaxLength(255);
            entity.Property(a => a.PhoneNumber).HasMaxLength(20);
            entity.Property(a => a.Department).HasMaxLength(100);
            entity.Property(a => a.Status).HasConversion<string>();
            entity.HasIndex(a => a.Email).IsUnique();
            entity.HasIndex(a => a.Status);
        });

        modelBuilder.Entity<TicketAssignment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AssignedBy).HasMaxLength(100);
           entity.Property(a => a.Title).HasMaxLength(1000);
            entity.Property(a => a.Description).HasMaxLength(5000);


            entity.HasOne(a => a.Agent)
                  .WithMany(ag => ag.Assignments)
                  .HasForeignKey(a => a.AgentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketResponse>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Content).IsRequired().HasMaxLength(5000);
            entity.Property(r => r.Resolution).HasMaxLength(1000);
            entity.Property(r => r.AgentName).HasMaxLength(200);
            entity.Property(r => r.AgentId).HasMaxLength(36);

            entity.HasOne(r => r.Agent)
                  .WithMany()
                  .HasForeignKey(r => r.AgentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}