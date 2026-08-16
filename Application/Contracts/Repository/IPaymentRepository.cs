using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId);
    Task<IEnumerable<Payment>> GetAllWithCustomerAsync();
    Task<IReadOnlyList<Payment>> GetAllForBackupAsync();
}
