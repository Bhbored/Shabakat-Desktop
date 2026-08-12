using Shabakat.Application.DTOs.Invoices;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IInvoiceRepository : IGenericRepository<Invoice>
{
    Task<int> GetNextInvoiceNumberAsync();
    Task<Invoice?> GetByIdWithPaymentsAsync(Guid id);
    Task<Invoice?> GetByIdForPrintAsync(Guid id);
    Task<(IEnumerable<Invoice> Items, int TotalCount)> GetAllPagedAsync(InvoiceFilterRequest filter);
    Task<IEnumerable<Invoice>> GetAllWithCustomerAsync();
    Task<bool> ExistsForCustomerInPeriodAsync(Guid customerId, DateOnly periodStart, DateOnly periodEnd);
    Task<Invoice?> GetByIdForUpdateAsync(Guid id);
}
