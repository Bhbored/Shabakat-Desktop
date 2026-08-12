using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence.Interceptors;

namespace Shabakat.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppPreferences> AppPreferences => Set<AppPreferences>();
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "shabakat.db3");
        optionsBuilder.UseSqlite($"Data Source={databasePath}");
        optionsBuilder.AddInterceptors(new AuditInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
