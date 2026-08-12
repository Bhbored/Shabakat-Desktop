using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.DTOs.AuditLogs;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;

    public AuditLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(AuditLog auditLog)
        => await _db.AuditLogs.AddAsync(auditLog);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllPagedAsync(
        AuditLogFilterRequest filter)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (filter.Action.HasValue)
            query = query.Where(a => a.Action == filter.Action.Value);

        if (filter.Status.HasValue)
            query = query.Where(a => a.Status == filter.Status.Value);

        if (filter.CreatedFrom.HasValue)
            query = query.Where(a => a.CreatedAt >= filter.CreatedFrom.Value);

        if (filter.CreatedTo.HasValue)
            query = query.Where(a => a.CreatedAt <= filter.CreatedTo.Value);

        var totalCount = await query.CountAsync();
        var pageSize = Math.Max(1, filter.PageSize);
        var pageNumber = Math.Max(1, filter.PageNumber);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
