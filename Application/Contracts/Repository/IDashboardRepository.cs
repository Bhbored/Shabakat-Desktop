using Shabakat.Application.DTOs.Dashboard;
using Shabakat.Application.DTOs.Invoices;

namespace Shabakat.Application.Contracts.Repository;

public interface IDashboardRepository
{
    Task<CustomerOverview> GetCustomerOverviewAsync();
    Task<InvoiceOverview> GetInvoiceOverviewAsync(DateOnly? periodStart = null, DateOnly? periodEndExclusive = null);
    Task<decimal> GetTotalOutstandingAsync(DateOnly? periodStart = null, DateOnly? periodEndExclusive = null);
    Task<ExpensesByType> GetExpensesByTypeAsync(DateOnly? periodStart = null, DateOnly? periodEndExclusive = null);
    Task<decimal> GetTotalCollectedAsync(DateOnly? periodStart = null, DateOnly? periodEndExclusive = null);
    Task<IReadOnlyList<InvoiceSummaryResponse>> GetRecentlyPaidAsync(int take = 5);
    Task<IReadOnlyList<InvoiceSummaryResponse>> GetUpcomingDueAsync(int take = 5);
}
