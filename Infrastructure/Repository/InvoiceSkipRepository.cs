using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class InvoiceSkipRepository : GenericRepository<InvoiceSkip>, IInvoiceSkipRepository
{
    public InvoiceSkipRepository(AppDbContext db) : base(db) { }

    public async Task UpsertAsync(InvoiceSkip skip)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(s =>
            s.CustomerId == skip.CustomerId &&
            s.BillingPeriodStart == skip.BillingPeriodStart &&
            s.BillingPeriodEnd == skip.BillingPeriodEnd);

        if (existing is null)
        {
            await _dbSet.AddAsync(skip);
            return;
        }

        existing.Reason = skip.Reason;
        existing.CustomerName = skip.CustomerName;
    }

    public async Task DeleteForCustomerPeriodAsync(
        Guid customerId, DateOnly billingPeriodStart, DateOnly billingPeriodEnd)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(s =>
            s.CustomerId == customerId &&
            s.BillingPeriodStart == billingPeriodStart &&
            s.BillingPeriodEnd == billingPeriodEnd);

        if (existing is not null)
            _dbSet.Remove(existing);
    }

    public new async Task<IEnumerable<InvoiceSkip>> GetAllAsync()
        => await _dbSet
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
}
