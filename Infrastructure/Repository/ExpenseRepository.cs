using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.DTOs.Expenses;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class ExpenseRepository : GenericRepository<Expenses>, IExpenseRepository
{
    public ExpenseRepository(AppDbContext db) : base(db) { }

    private IQueryable<Expenses> ApplyFilter(ExpenseFilterRequest filter)
    {
        var query = _dbSet.AsQueryable();

        if (filter.DateFrom.HasValue)
            query = query.Where(e => e.ExpenseDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(e => e.ExpenseDate <= filter.DateTo.Value);

        if (filter.ExpenseType.HasValue)
            query = query.Where(e => e.ExpenseType == filter.ExpenseType.Value);

        return query;
    }

    public async Task<(IEnumerable<Expenses> Items, int TotalCount)> GetAllPagedAsync(
        ExpenseFilterRequest filter)
    {
        var query = ApplyFilter(filter);
        var totalCount = await query.CountAsync();
        var pageSize = Math.Max(1, filter.PageSize);
        var pageNumber = Math.Max(1, filter.PageNumber);

        var items = await query
            .OrderByDescending(e => e.ExpenseDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Expenses>> GetAllMatchingAsync(ExpenseFilterRequest filter)
        => await ApplyFilter(filter).ToListAsync();
}
