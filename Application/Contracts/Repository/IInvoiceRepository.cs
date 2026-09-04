using Shabakat.Application.DTOs.Invoices;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IInvoiceRepository : IGenericRepository<Invoice>
{
    Task<int> GetNextInvoiceNumberAsync();
    Task<Invoice?> GetByIdWithPaymentsAsync(Guid id);
    Task<Invoice?> GetByIdForPrintAsync(Guid id);
    Task<(IEnumerable<Invoice> Items, int TotalCount)> GetAllPagedAsync(
        InvoiceFilterRequest filter,
        int paymentDueDay,
        DateOnly today);
    Task<IEnumerable<Invoice>> GetAllWithCustomerAsync();
    Task<IReadOnlyList<Invoice>> GetForIssueDateRangesAsync(
        DateOnly selectedMonthStart,
        DateOnly selectedMonthEnd,
        DateOnly previousMonthStart,
        DateOnly previousMonthEnd);
    Task<bool> ExistsForCustomerInPeriodAsync(Guid customerId, DateOnly periodStart, DateOnly periodEnd);
    Task<Invoice?> GetByIdForUpdateAsync(Guid id);
}
