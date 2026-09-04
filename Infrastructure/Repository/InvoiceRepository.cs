using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.DTOs.Invoices;
using Shabakat.Application.Helper;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

    public async Task<int> GetNextInvoiceNumberAsync()
        => (await _dbSet.MaxAsync(i => (int?)i.InvoiceNumber) ?? 0) + 1;

    public async Task<Invoice?> GetByIdWithPaymentsAsync(Guid id)
        => await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Invoice?> GetByIdForPrintAsync(Guid id)
        => await _dbSet
            .Include(i => i.Customer)
                .ThenInclude(c => c!.AmpereSchedule)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<(IEnumerable<Invoice> Items, int TotalCount)> GetAllPagedAsync(
        InvoiceFilterRequest filter,
        int paymentDueDay,
        DateOnly today)
    {
        var query = _dbSet
            .Include(i => i.Customer)
            .AsQueryable();

        if (filter.CustomerId.HasValue)
            query = query.Where(i => i.CustomerId == filter.CustomerId.Value);

        if (filter.InvoiceStatus.HasValue)
            query = query.Where(i => i.InvoiceStatus == filter.InvoiceStatus.Value);

        if (filter.ConsumptionStartFrom.HasValue)
            query = query.Where(i => i.IssueDate >= filter.ConsumptionStartFrom.Value);

        if (filter.ConsumptionStartTo.HasValue)
            query = query.Where(i => i.IssueDate <= filter.ConsumptionStartTo.Value);

        if (filter.DueTiming.HasValue)
        {
            query = query.Where(i => i.AmountDue > 0);

            var currentDue = BillingPeriodHelper.ResolveScheduledPaymentDueDate(today, paymentDueDay);
            var previousDue = BillingPeriodHelper.ResolveScheduledPaymentDueDate(today.AddMonths(-1), paymentDueDay);
            var nextDue = BillingPeriodHelper.ResolveScheduledPaymentDueDate(today.AddMonths(1), paymentDueDay);
            var tomorrowStart = today.AddDays(1).ToDateTime(TimeOnly.MinValue);

            switch (filter.DueTiming.Value)
            {
                case InvoiceDueTimingFilter.Overdue:
                {
                    var latestPastDue = currentDue < today ? currentDue : previousDue;
                    var overdueCutoff = latestPastDue.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    query = query.Where(i => i.CreatedAt < overdueCutoff);
                    break;
                }
                case InvoiceDueTimingFilter.DueToday:
                    if (currentDue != today)
                    {
                        query = query.Where(_ => false);
                    }
                    else
                    {
                        var cycleStart = previousDue.AddDays(1).ToDateTime(TimeOnly.MinValue);
                        query = query.Where(i => i.CreatedAt >= cycleStart && i.CreatedAt < tomorrowStart);
                    }
                    break;
                case InvoiceDueTimingFilter.DueSoon:
                {
                    var upcomingDue = currentDue > today ? currentDue : nextDue;
                    if (upcomingDue > today.AddDays(7))
                    {
                        query = query.Where(_ => false);
                    }
                    else
                    {
                        var priorDue = upcomingDue == currentDue ? previousDue : currentDue;
                        var cycleStart = priorDue.AddDays(1).ToDateTime(TimeOnly.MinValue);
                        query = query.Where(i => i.CreatedAt >= cycleStart && i.CreatedAt < tomorrowStart);
                    }
                    break;
                }
            }
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Max(1, filter.PageSize);
        var pageNumber = Math.Max(1, filter.PageNumber);

        var items = await query
            .OrderByDescending(i => i.InvoiceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Invoice>> GetAllWithCustomerAsync()
        => await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Payments)
                .ThenInclude(p => p.Customer)
            .OrderByDescending(i => i.InvoiceNumber)
            .ToListAsync();

    public async Task<IReadOnlyList<Invoice>> GetForIssueDateRangesAsync(
        DateOnly selectedMonthStart,
        DateOnly selectedMonthEnd,
        DateOnly previousMonthStart,
        DateOnly previousMonthEnd)
        => await _dbSet
            .AsNoTracking()
            .Include(i => i.Customer)
            .Where(i =>
                (i.Customer.Plan == PlanType.Ampere
                    && i.IssueDate >= selectedMonthStart
                    && i.IssueDate <= selectedMonthEnd)
                || (i.Customer.Plan == PlanType.Kilowatt
                    && i.IssueDate >= previousMonthStart
                    && i.IssueDate <= previousMonthEnd))
            .OrderBy(i => i.IssueDate)
            .ThenBy(i => i.InvoiceNumber)
            .ToListAsync();

    public async Task<bool> ExistsForCustomerInPeriodAsync(
        Guid customerId, DateOnly periodStart, DateOnly periodEnd)
        => await _dbSet.AnyAsync(i =>
            i.CustomerId == customerId &&
            i.IssueDate >= periodStart &&
            i.IssueDate <= periodEnd);

    public async Task<Invoice?> GetByIdForUpdateAsync(Guid id)
        => await _dbSet.FirstOrDefaultAsync(i => i.Id == id);
}
