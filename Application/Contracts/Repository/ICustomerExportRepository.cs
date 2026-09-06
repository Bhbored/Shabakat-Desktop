using Shabakat.Application.DTOs.Exports;

namespace Shabakat.Application.Contracts.Repository;

public interface ICustomerExportRepository
{
    Task<IReadOnlyList<ExportAreaRef>> GetAreasAsync(
        IReadOnlyCollection<Guid>? areaIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerExportRow>> GetRowsForAreaAsync(
        Guid areaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerExportRow>> GetRowsWithoutAreaAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerExportRow>> GetRowsAsync(
        IReadOnlyCollection<Guid>? areaIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExportBoxRow>> GetBoxesForAreaAsync(
        Guid areaId,
        CancellationToken cancellationToken = default);
}
