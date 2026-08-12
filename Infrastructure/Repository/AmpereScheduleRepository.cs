using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class AmpereScheduleRepository : GenericRepository<AmpereSchedule>, IAmpereScheduleRepository
{
    public AmpereScheduleRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<AmpereSchedule>> GetAllWithCustomerCountAsync()
        => await _dbSet
            .Include(s => s.Customers)
            .OrderBy(s => s.HoursPerDay)
            .ToListAsync();

    public async Task<AmpereSchedule?> GetByIdWithDetailsAsync(Guid id)
        => await _dbSet
            .Include(s => s.Customers)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<bool> HasCustomersAsync(Guid scheduleId)
        => await _db.Set<Customer>()
            .AnyAsync(c => c.AmpereScheduleId == scheduleId);

    public async Task<bool> HasAnyAsync()
        => await _dbSet.AnyAsync();

    public async Task<bool> HoursPerDayExistsAsync(int hoursPerDay, Guid? excludeId = null)
        => await _dbSet.AnyAsync(s =>
            s.HoursPerDay == hoursPerDay &&
            (!excludeId.HasValue || s.Id != excludeId.Value));
}
