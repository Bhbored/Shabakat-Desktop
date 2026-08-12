using Shabakat.Application.DTOs.Customers;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByIdWithInvoicesAsync(Guid id);
    Task<IEnumerable<Customer>> GetActiveWithoutInvoiceAsync(
        DateOnly amperePeriodStart,
        DateOnly amperePeriodEnd,
        DateOnly kilowattPeriodStart,
        DateOnly kilowattPeriodEnd);
    Task<IEnumerable<Customer>> GetActiveAmpereReadyForNextMonthAsync(
        DateOnly currentPeriodStart,
        DateOnly currentPeriodEnd,
        DateOnly nextPeriodStart,
        DateOnly nextPeriodEnd);
    Task<(IEnumerable<Customer> Items, int TotalCount)> GetAllWithCurrentMonthInvoicesAsync(
        CustomerFilterRequest filter);
    Task<IEnumerable<Customer>> GetAllWithDetailsAsync();
    Task<IReadOnlyList<Customer>> GetByIdsAsync(IEnumerable<Guid> ids);
}
