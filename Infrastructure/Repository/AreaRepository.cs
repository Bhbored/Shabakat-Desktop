using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class AreaRepository : GenericRepository<Area>, IAreaRepository
{
    public AreaRepository(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

    public async Task<IEnumerable<Area>> GetAllWithCustomerCountAsync()
        => await _dbSet
            .Include(a => a.Customers)
            .Include(a => a.DistributionBoxes)
            .OrderBy(a => a.Name)
            .ToListAsync();

    public async Task<bool> HasCustomersAsync(Guid areaId)
        => await _db.Set<Customer>()
            .AnyAsync(c => c.AreaId == areaId);
}
