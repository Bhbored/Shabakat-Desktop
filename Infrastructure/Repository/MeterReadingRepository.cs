using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class MeterReadingRepository : GenericRepository<MeterReading>, IMeterReadingRepository
{
    public MeterReadingRepository(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

    public async Task<MeterReading?> GetLatestBeforeAsync(
        Guid customerId, DateOnly beforeDate, Guid? excludeReadingId = null)
    {
        var query = _dbSet.Where(m => m.CustomerId == customerId);

        if (excludeReadingId is Guid id)
            query = query.Where(m => m.Id != id);

        return await query
            .Where(m =>
                m.ReadingDate < beforeDate
                || (m.ReadingDate == beforeDate && m.IsInitial))
            .OrderByDescending(m => m.ReadingDate)
            .ThenBy(m => m.IsInitial)
            .ThenByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<MeterReading>> GetAllForCustomerAsync(Guid customerId)
        => await _dbSet
            .Where(m => m.CustomerId == customerId)
            .OrderByDescending(m => m.ReadingDate)
            .ThenBy(m => m.IsInitial)
            .ToListAsync();

    public async Task<IEnumerable<MeterReading>> GetAllWithCustomerAsync()
        => await _dbSet
            .Include(m => m.Customer)
            .OrderByDescending(m => m.ReadingDate)
            .ThenBy(m => m.IsInitial)
            .ThenByDescending(m => m.CreatedAt)
            .ToListAsync();

    public async Task<MeterReading?> GetLatestForCustomerAsync(Guid customerId)
        => await _dbSet
            .Where(m => m.CustomerId == customerId)
            .OrderByDescending(m => m.ReadingDate)
            .ThenBy(m => m.IsInitial)
            .ThenByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task<MeterReading?> GetForPeriodAsync(
        Guid customerId, DateOnly periodStart, DateOnly periodEnd)
        => await _dbSet
            .Where(m => m.CustomerId == customerId
                     && !m.IsInitial
                     && m.ReadingDate >= periodStart
                     && m.ReadingDate < periodEnd)
            .OrderByDescending(m => m.ReadingDate)
            .ThenByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task<MeterReading?> GetInitialForCustomerAsync(Guid customerId)
        => await _dbSet
            .Where(m => m.CustomerId == customerId && m.IsInitial)
            .FirstOrDefaultAsync();

    public async Task<bool> HasOfficialReadingAsync(Guid customerId)
        => await _dbSet.AnyAsync(m => m.CustomerId == customerId && !m.IsInitial);

    public async Task<IReadOnlyList<MeterReading>> GetAllForBackupAsync()
        => await _dbSet.AsNoTracking().ToListAsync();
}
