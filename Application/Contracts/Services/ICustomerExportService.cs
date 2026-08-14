using Shabakat.Application.DTOs.Exports;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Contracts.Services;

public interface ICustomerExportService
{
    Task<CustomerExportFile> BuildAsync(
        CustomerExportRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerExportColumn>> GetSelectedColumnsAsync(
        CancellationToken cancellationToken = default);

    Task SaveSelectedColumnsAsync(
        IReadOnlyCollection<CustomerExportColumn> columns,
        CancellationToken cancellationToken = default);
}
