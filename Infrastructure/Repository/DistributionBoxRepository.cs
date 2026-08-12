using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.DTOs.DistributionBox;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class DistributionBoxRepository : GenericRepository<DistributionBox>, IDistributionBoxRepository
{
    public DistributionBoxRepository(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

    public async Task<(IEnumerable<DistributionBox> Items, int TotalCount)> GetAllPagedAsync(
        DistributionBoxFilterRequest filter)
    {
        var query = BuildDetailsQuery();

        if (filter.AreaId.HasValue)
            query = query.Where(b => b.AreaId == filter.AreaId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.Trim().ToLower();
            query = query.Where(b => b.Name.ToLower().Contains(name));
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Max(1, filter.PageSize);
        var pageNumber = Math.Max(1, filter.PageNumber);

        var items = await query
            .OrderBy(b => b.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<DistributionBox>> GetAllWithDetailsAsync()
        => await BuildDetailsQuery()
            .OrderBy(b => b.Name)
            .ToListAsync();

    public async Task<DistributionBox?> GetByIdWithDetailsAsync(Guid id)
        => await BuildDetailsQuery()
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<bool> HasCustomersAsync(Guid boxId)
        => await _db.Set<Customer>()
            .AnyAsync(c => c.BoxId == boxId);

    private IQueryable<DistributionBox> BuildDetailsQuery()
        => _dbSet
            .Include(b => b.Area)
            .Include(b => b.Customers);
}
