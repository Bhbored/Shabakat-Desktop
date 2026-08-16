using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Dashboard;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IDashboardRepository dashboardRepository,
        ILogger<DashboardService> logger)
    {
        _dashboardRepository = dashboardRepository;
        _logger = logger;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(int? year = null, int? month = null)
    {
        var (periodStart, periodEndExclusive) = ResolvePeriod(year, month);
        _logger.LogDebug(
            "Loading dashboard summary for {PeriodStart}..{PeriodEnd}",
            periodStart,
            periodEndExclusive);

        var customers = await _dashboardRepository.GetCustomerOverviewAsync();
        var invoiceOverview = await _dashboardRepository.GetInvoiceOverviewAsync(
            periodStart, periodEndExclusive);
        var outstanding = await _dashboardRepository.GetTotalOutstandingAsync(
            periodStart, periodEndExclusive);
        var expensesByType = await _dashboardRepository.GetExpensesByTypeAsync(
            periodStart, periodEndExclusive);

        var totalBilled = invoiceOverview.UnpaidTotal
            + invoiceOverview.PartiallyPaidTotal
            + invoiceOverview.PaidTotal;
        var totalCollected = await _dashboardRepository.GetTotalCollectedAsync(
            periodStart, periodEndExclusive);

        var totalExpenses = expensesByType.Fuel + expensesByType.Maintenance
                           + expensesByType.Employees + expensesByType.Other;

        var collectionRate = totalBilled > 0
            ? Math.Round(totalCollected / totalBilled * 100, 2)
            : 0;

        var netIncome = totalCollected - totalExpenses;

        var recentlyPaid = await _dashboardRepository.GetRecentlyPaidAsync();
        var upcomingDue = await _dashboardRepository.GetUpcomingDueAsync();

        return new DashboardSummaryResponse(
            TotalBilledAllTime: totalBilled,
            TotalCollectedAllTime: totalCollected,
            TotalOutstandingAllTime: outstanding,
            CollectionRate: collectionRate,
            TotalExpensesAllTime: totalExpenses,
            NetIncomeAllTime: netIncome,
            Customers: customers,
            Invoices: invoiceOverview,
            ExpensesByType: expensesByType,
            RecentlyPaid: recentlyPaid,
            UpcomingDue: upcomingDue);
    }

    private static (DateOnly? PeriodStart, DateOnly? PeriodEndExclusive) ResolvePeriod(
        int? year,
        int? month)
    {
        if (!year.HasValue && !month.HasValue)
            return (null, null);

        if (!year.HasValue || !month.HasValue)
        {
            throw new DomainException("Error.DashboardPeriodIncomplete");
        }

        if (month is < 1 or > 12)
            throw new DomainException("Error.MonthRange");

        if (year is < 2000 or > 2100)
            throw new DomainException("Error.YearRange");

        var periodStart = new DateOnly(year.Value, month.Value, 1);
        return (periodStart, periodStart.AddMonths(1));
    }
}
