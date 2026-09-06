using Shabakat.Application.DTOs.Exports;

namespace Shabakat.Application.Contracts.Services;

public interface IUnpaidInvoiceExportService
{
    Task<UnpaidInvoiceExportFile?> BuildAsync(CancellationToken cancellationToken = default);
}
