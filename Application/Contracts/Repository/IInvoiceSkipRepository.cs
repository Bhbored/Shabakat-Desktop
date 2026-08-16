using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IInvoiceSkipRepository : IGenericRepository<InvoiceSkip>
{
    Task UpsertAsync(InvoiceSkip skip);
    Task DeleteForCustomerPeriodAsync(
        Guid customerId, DateOnly billingPeriodStart, DateOnly billingPeriodEnd);
    Task<IReadOnlyList<InvoiceSkip>> GetAllForBackupAsync();
}
