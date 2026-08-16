using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Backup;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class BackupRepository : IBackupRepository
{
    private const int LoadSteps = 14;
    private const int ReplaceSteps = 28;

    private readonly AppDbContext _db;
    private readonly ILogger<BackupRepository> _logger;

    public BackupRepository(AppDbContext db, ILogger<BackupRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async IAsyncEnumerable<double> LoadAsync(
        BackupFile destination,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var step = 0;

        destination.AppUser = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.Preferences = await _db.AppPreferences.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.ExportColumns = await _db.CustomerExportColumnPreferences.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.Areas = await _db.Areas.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.DistributionBoxes = await _db.DistributionBoxes.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.AmpereSchedules = await _db.AmpereSchedules.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.Customers = await _db.Customers.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.MeterReadings = await _db.MeterReadings.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.Invoices = await _db.Invoices.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.Payments = await _db.Payments.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.InvoiceSkips = await _db.InvoiceSkips.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.Expenses = await _db.Expenses.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.AuditLogs = await _db.AuditLogs.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);

        destination.AuditLogDetails = await _db.AuditLogDetails.AsNoTracking().ToListAsync(cancellationToken);
        yield return Progress(++step, LoadSteps);
    }

    public async IAsyncEnumerable<double> ReplaceAsync(
        BackupFile file,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _db.ChangeTracker.Clear();
        _db.PreserveTimestamps = true;

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var step = 0;
        try
        {
            await _db.Database.ExecuteSqlRawAsync("PRAGMA defer_foreign_keys = ON;", cancellationToken);

            await _db.Payments.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.MeterReadings.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.InvoiceSkips.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.AuditLogDetails.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.AuditLogs.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.Invoices.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.Expenses.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.Customers.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.DistributionBoxes.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.Areas.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.AmpereSchedules.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.CustomerExportColumnPreferences.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);
            await _db.AppPreferences.ExecuteDeleteAsync(cancellationToken);
            yield return Progress(++step, ReplaceSteps);

            if (file.AppUser is not null)
            {
                await _db.AppUsers.ExecuteDeleteAsync(cancellationToken);
                await _db.AppUsers.AddAsync(file.AppUser, cancellationToken);
            }

            yield return Progress(++step, ReplaceSteps);

            if (file.Preferences is not null)
            {
                file.Preferences.CustomerExportColumnPreference = null;
                await _db.AppPreferences.AddAsync(file.Preferences, cancellationToken);
            }

            yield return Progress(++step, ReplaceSteps);

            AddRange(file.ExportColumns);
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.Areas);
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.AmpereSchedules);
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.DistributionBoxes);
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.Customers);
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.Invoices);
            foreach (var invoice in file.Invoices)
                _db.Entry(invoice).Property(i => i.AmountDue).IsModified = false;
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.Expenses);
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.MeterReadings);
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.InvoiceSkips);
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.Payments);
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.AuditLogs);
            yield return Progress(++step, ReplaceSteps);
            AddRange(file.AuditLogDetails);
            yield return Progress(++step, ReplaceSteps);

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Restored backup");
            yield return Progress(++step, ReplaceSteps);
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

    private static double Progress(int step, int total)
        => Math.Clamp(step / (double)total, 0d, 1d);
}
