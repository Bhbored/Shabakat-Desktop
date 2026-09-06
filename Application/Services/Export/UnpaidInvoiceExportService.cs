using Shabakat.Application.Contracts.Abstractions;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Exports;
using Shabakat.Application.Helper;
using Shabakat.Application.Mappers;

namespace Shabakat.Application.Services.Export;

public sealed class UnpaidInvoiceExportService(
    IUnpaidInvoiceExportRepository exportRepository,
    IUnpaidInvoiceExportWorkbookBuilder workbookBuilder,
    IAppPreferencesRepository preferencesRepository)
    : IUnpaidInvoiceExportService
{
    private const string XlsxContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public async Task<UnpaidInvoiceExportFile?> BuildAsync(CancellationToken cancellationToken = default)
    {
        var preferences = await preferencesRepository.GetAsync();
        var paymentDueDay = preferences?.DueDate ?? 31;
        var rows = await exportRepository.GetOutstandingRowsAsync(paymentDueDay, cancellationToken);

        if (rows.Count == 0)
            return null;

        var exportedAt = DateTime.Now;
        var customerColumns = preferences?.CustomerExportColumnPreference is { } columnPreference
            ? columnPreference.ToSelectedColumns()
            : CustomerExportColumns.Default;
        var columns = UnpaidInvoiceExportColumns.Resolve(customerColumns);

        return new UnpaidInvoiceExportFile(
            workbookBuilder.Build(rows, columns, exportedAt, preferences?.Language),
            $"unpaid-invoices-{exportedAt:yyyyMMdd-HHmm}.xlsx",
            XlsxContentType);
    }
}
