using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Backup;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class BackupRepository : IBackupRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<BackupRepository> _logger;

    public BackupRepository(AppDbContext db, ILogger<BackupRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<BackupFile> LoadAsync() =>
        new()
        {
            AppUser = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync(),
            Preferences = await _db.AppPreferences.AsNoTracking().FirstOrDefaultAsync(),
            ExportColumns = await _db.CustomerExportColumnPreferences.AsNoTracking().ToListAsync(),
            Areas = await _db.Areas.AsNoTracking().ToListAsync(),
            DistributionBoxes = await _db.DistributionBoxes.AsNoTracking().ToListAsync(),
            AmpereSchedules = await _db.AmpereSchedules.AsNoTracking().ToListAsync(),
            Customers = await _db.Customers.AsNoTracking().ToListAsync(),
            MeterReadings = await _db.MeterReadings.AsNoTracking().ToListAsync(),
            Invoices = await _db.Invoices.AsNoTracking().ToListAsync(),
            Payments = await _db.Payments.AsNoTracking().ToListAsync(),
            InvoiceSkips = await _db.InvoiceSkips.AsNoTracking().ToListAsync(),
            Expenses = await _db.Expenses.AsNoTracking().ToListAsync(),
            AuditLogs = await _db.AuditLogs.AsNoTracking().ToListAsync(),
            AuditLogDetails = await _db.AuditLogDetails.AsNoTracking().ToListAsync()
        };

    public async Task ReplaceAsync(BackupFile file)
    {
        _db.ChangeTracker.Clear();
        _db.PreserveTimestamps = true;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _db.Database.ExecuteSqlRawAsync("PRAGMA defer_foreign_keys = ON;");

            await _db.Payments.ExecuteDeleteAsync();
            await _db.MeterReadings.ExecuteDeleteAsync();
            await _db.InvoiceSkips.ExecuteDeleteAsync();
            await _db.AuditLogDetails.ExecuteDeleteAsync();
            await _db.AuditLogs.ExecuteDeleteAsync();
            await _db.Invoices.ExecuteDeleteAsync();
            await _db.Expenses.ExecuteDeleteAsync();
            await _db.Customers.ExecuteDeleteAsync();
            await _db.DistributionBoxes.ExecuteDeleteAsync();
            await _db.Areas.ExecuteDeleteAsync();
            await _db.AmpereSchedules.ExecuteDeleteAsync();
            await _db.CustomerExportColumnPreferences.ExecuteDeleteAsync();
            await _db.AppPreferences.ExecuteDeleteAsync();
            await _db.AppUsers.ExecuteDeleteAsync();

            if (file.AppUser is not null)
                await _db.AppUsers.AddAsync(file.AppUser);

            if (file.Preferences is not null)
            {
                file.Preferences.CustomerExportColumnPreference = null;
                await _db.AppPreferences.AddAsync(file.Preferences);
            }

            AddRange(file.ExportColumns);
            AddRange(file.Areas);
            AddRange(file.AmpereSchedules);
            AddRange(file.DistributionBoxes);
            AddRange(file.Customers);
            AddRange(file.Invoices);
            foreach (var invoice in file.Invoices)
                _db.Entry(invoice).Property(i => i.AmountDue).IsModified = false;

            AddRange(file.Expenses);
            AddRange(file.MeterReadings);
            AddRange(file.InvoiceSkips);
            AddRange(file.Payments);
            AddRange(file.AuditLogs);
            AddRange(file.AuditLogDetails);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation("Restored backup");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Backup restore rolled back");
            throw;
        }
        finally
        {
            _db.PreserveTimestamps = false;
            _db.ChangeTracker.Clear();
        }
    }

    private void AddRange<T>(IReadOnlyList<T> entities) where T : class
    {
        if (entities.Count == 0)
            return;

        _db.Set<T>().AddRange(entities);
    }
}
