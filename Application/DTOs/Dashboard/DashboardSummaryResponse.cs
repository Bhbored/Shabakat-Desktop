using Shabakat.Application.DTOs.Invoices;

namespace Shabakat.Application.DTOs.Dashboard;

public record DashboardSummaryResponse(
    decimal TotalBilledAllTime,
    decimal TotalCollectedAllTime,
    decimal TotalOutstandingAllTime,
    decimal CollectionRate,
    decimal TotalExpensesAllTime,
    decimal NetIncomeAllTime,
    CustomerOverview Customers,
    InvoiceOverview Invoices,
    ExpensesByType ExpensesByType,
    IReadOnlyList<InvoiceSummaryResponse> RecentlyPaid,
    IReadOnlyList<InvoiceSummaryResponse> UpcomingDue);
