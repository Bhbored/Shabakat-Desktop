using Shabakat.Application.DTOs.Exports;

namespace Shabakat.Application.Contracts.Repository;

public interface IUnpaidInvoiceExportRepository
{
    Task<IReadOnlyList<UnpaidInvoiceExportRow>> GetOutstandingRowsAsync(
        int paymentDueDay,
        CancellationToken cancellationToken = default);
}
