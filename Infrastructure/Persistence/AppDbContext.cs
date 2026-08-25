using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public bool PreserveTimestamps { get; set; }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppPreferences> AppPreferences => Set<AppPreferences>();
    public DbSet<CustomerExportColumnPreference> CustomerExportColumnPreferences => Set<CustomerExportColumnPreference>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expenses> Expenses => Set<Expenses>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<DistributionBox> DistributionBoxes => Set<DistributionBox>();
    public DbSet<AmpereSchedule> AmpereSchedules => Set<AmpereSchedule>();
    public DbSet<InvoiceSkip> InvoiceSkips => Set<InvoiceSkip>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AuditLogDetails> AuditLogDetails => Set<AuditLogDetails>();
    public DbSet<CloudBackupState> CloudBackupStates => Set<CloudBackupState>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
