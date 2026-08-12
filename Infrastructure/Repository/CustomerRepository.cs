using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.DTOs.Customers;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

    public async Task<Customer?> GetByIdWithInvoicesAsync(Guid id)
        => await _dbSet
            .Include(c => c.Invoices)
            .Include(c => c.Area)
            .Include(c => c.DistributionBox)
            .Include(c => c.AmpereSchedule)
            .Include(c => c.MeterReadings.Where(m => m.IsInitial))
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Customer>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.Distinct().ToList();
        return await _dbSet
            .Where(c => idList.Contains(c.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetActiveWithoutInvoiceAsync(
        DateOnly amperePeriodStart,
        DateOnly amperePeriodEnd,
        DateOnly kilowattPeriodStart,
        DateOnly kilowattPeriodEnd)
        => await _dbSet
            .Where(c => c.CustomerStatus == CustomerStatus.Active
                     && (
                         (c.Plan == PlanType.Ampere &&
                          !c.Invoices.Any(i =>
                              i.IssueDate >= amperePeriodStart &&
                              i.IssueDate <= amperePeriodEnd))
                         ||
                         (c.Plan == PlanType.Kilowatt &&
                          !c.Invoices.Any(i =>
                              i.IssueDate >= kilowattPeriodStart &&
                              i.IssueDate <= kilowattPeriodEnd))
                     ))
            .Include(c => c.AmpereSchedule)
            .ToListAsync();

    public async Task<IEnumerable<Customer>> GetActiveAmpereReadyForNextMonthAsync(
        DateOnly currentPeriodStart,
        DateOnly currentPeriodEnd,
        DateOnly nextPeriodStart,
        DateOnly nextPeriodEnd)
        => await _dbSet
            .Where(c => c.CustomerStatus == CustomerStatus.Active
                     && c.Plan == PlanType.Ampere
                     && c.Invoices.Any(i =>
                         i.IssueDate >= currentPeriodStart &&
                         i.IssueDate <= currentPeriodEnd)
                     && !c.Invoices.Any(i =>
                         i.IssueDate >= nextPeriodStart &&
                         i.IssueDate <= nextPeriodEnd))
            .Include(c => c.AmpereSchedule)
            .ToListAsync();

    public async Task<(IEnumerable<Customer> Items, int TotalCount)>
        GetAllWithCurrentMonthInvoicesAsync(CustomerFilterRequest filter)
    {
        var now = DateTime.Now;
        var monthStart = new DateOnly(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => c.Name.Contains(filter.Name.Trim()));

        if (!string.IsNullOrWhiteSpace(filter.Phone))
            query = query.Where(c => c.Phone != null && c.Phone.Contains(filter.Phone.Trim()));

        if (filter.PlanType.HasValue)
            query = query.Where(c => c.Plan == filter.PlanType.Value);

        if (filter.CustomerRelation.HasValue)
            query = query.Where(c => c.CustomerRelation == filter.CustomerRelation.Value);

        if (filter.CustomerStatus.HasValue)
            query = query.Where(c => c.CustomerStatus == filter.CustomerStatus.Value);

        if (filter.AreaId.HasValue)
            query = query.Where(c => c.AreaId == filter.AreaId.Value);

        if (filter.BoxId.HasValue)
            query = query.Where(c => c.BoxId == filter.BoxId.Value);

        if (filter.AmpereScheduleId.HasValue)
            query = query.Where(c => c.AmpereScheduleId == filter.AmpereScheduleId.Value);

        if (!string.IsNullOrWhiteSpace(filter.PaymentFilter))
        {
            if (filter.PaymentFilter.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.Invoices.Any(i =>
                    i.IssueDate >= monthStart &&
                    i.IssueDate < monthEnd &&
                    i.InvoiceStatus == InvoiceStatus.Paid));
            else if (filter.PaymentFilter.Equals("Unpaid", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.Invoices.Any(i =>
                    i.IssueDate >= monthStart &&
                    i.IssueDate < monthEnd &&
                    i.InvoiceStatus == InvoiceStatus.Unpaid));
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Max(1, filter.PageSize);
        var pageNumber = Math.Max(1, filter.PageNumber);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Include(c => c.Invoices)
            .Include(c => c.Area)
            .Include(c => c.DistributionBox)
            .Include(c => c.AmpereSchedule)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Customer>> GetAllWithDetailsAsync()
        => await _dbSet
            .OrderByDescending(c => c.CreatedAt)
            .Include(c => c.Invoices)
            .Include(c => c.Area)
            .Include(c => c.DistributionBox)
            .Include(c => c.AmpereSchedule)
            .Include(c => c.MeterReadings.Where(m => m.IsInitial))
            .ToListAsync();
}
