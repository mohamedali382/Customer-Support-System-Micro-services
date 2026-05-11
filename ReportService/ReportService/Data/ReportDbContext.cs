using Microsoft.EntityFrameworkCore;

using ReportService.Entities;
using System.Collections.Generic;

namespace ReportService.Data;

public class ReportDbContext : DbContext
{
    public ReportDbContext(DbContextOptions<ReportDbContext> options)
    : base(options) { }

    public DbSet<TicketSnapshot> TicketSnapshots => Set<TicketSnapshot>();
}