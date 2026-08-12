using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.DTOs.Dashboard;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _db;

    public DashboardRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CustomerOverview> GetCustomerOverviewAsync()
    {
        var customers = await _db.Customers
            .Select(c => new { c.CustomerStatus, c.Plan })
            .ToListAsync();

        return new CustomerOverview(
            Total: customers.Count,
            Active: customers.Count(c => c.CustomerStatus == CustomerStatus.Active),
            Suspended: customers.Count(c => c.CustomerStatus == CustomerStatus.Suspended),
            Terminated: customers.Count(c => c.CustomerStatus == CustomerStatus.Terminated),
            AmpereCount: customers.Count(c => c.Plan == PlanType.Ampere),
            KilowattCount: customers.Count(c => c.Plan == PlanType.Kilowatt));
    }

    public async Task<InvoiceOverview> GetInvoiceOverviewAsync(
        DateOnly? periodStart = null,
        DateOnly? periodEndExclusive = null)
    {
        var query = ApplyInvoicePeriodFilter(_db.Invoices.AsQueryable(), periodStart, periodEndExclusive);

        var invoices = await query
            .Select(i => new { i.InvoiceStatus, i.TotalAmount })
            .ToListAsync();

        var unpaid = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Unpaid).ToList();
        var partiallyPaid = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.PartiallyPaid).ToList();
        var paid = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Paid).ToList();

        return new InvoiceOverview(
            UnpaidCount: unpaid.Count,
            UnpaidTotal: unpaid.Sum(i => i.TotalAmount),
            PartiallyPaidCount: partiallyPaid.Count,
            PartiallyPaidTotal: partiallyPaid.Sum(i => i.TotalAmount),
            PaidCount: paid.Count,
            PaidTotal: paid.Sum(i => i.TotalAmount));
    }

    public async Task<decimal> GetTotalOutstandingAsync(
        DateOnly? periodStart = null,
        DateOnly? periodEndExclusive = null)
    {
        var query = _db.Invoices.Where(i => i.InvoiceStatus != InvoiceStatus.Paid);
        query = ApplyInvoicePeriodFilter(query, periodStart, periodEndExclusive);
        return await query.SumAsync(i => i.AmountDue);
    }

    public async Task<ExpensesByType> GetExpensesByTypeAsync(
        DateOnly? periodStart = null,
        DateOnly? periodEndExclusive = null)
    {
        var query = _db.Expenses.AsQueryable();

        if (periodStart.HasValue && periodEndExclusive.HasValue)
        {
            query = query.Where(e =>
                e.ExpenseDate >= periodStart.Value &&
                e.ExpenseDate < periodEndExclusive.Value);
        }

        var expenses = await query
            .Select(e => new { e.ExpenseType, e.Amount })
            .ToListAsync();

        return new ExpensesByType(
            Fuel: expenses.Where(e => e.ExpenseType == ExpenseType.Fuel).Sum(e => e.Amount),
            Maintenance: expenses.Where(e => e.ExpenseType == ExpenseType.Maintenance).Sum(e => e.Amount),
            Employees: expenses.Where(e => e.ExpenseType == ExpenseType.Employees).Sum(e => e.Amount),
            Other: expenses.Where(e => e.ExpenseType == ExpenseType.Other).Sum(e => e.Amount));
    }

    public async Task<decimal> GetTotalCollectedAsync(
        DateOnly? periodStart = null,
        DateOnly? periodEndExclusive = null)
    {
        var query = ApplyInvoicePeriodFilter(_db.Invoices.AsQueryable(), periodStart, periodEndExclusive);
        return await query.SumAsync(i => i.PaidAmount);
    }

    private static IQueryable<Invoice> ApplyInvoicePeriodFilter(
        IQueryable<Invoice> query,
        DateOnly? periodStart,
        DateOnly? periodEndExclusive)
    {
        if (!periodStart.HasValue || !periodEndExclusive.HasValue)
            return query;

        return query.Where(i =>
            i.IssueDate >= periodStart.Value &&
            i.IssueDate < periodEndExclusive.Value);
    }
}
