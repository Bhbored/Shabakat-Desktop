using Shabakat.Application.DTOs.Exports;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Contracts.Abstractions;

public interface IUnpaidInvoiceExportWorkbookBuilder
{
    byte[] Build(
        IReadOnlyList<UnpaidInvoiceExportRow> rows,
        IReadOnlyList<UnpaidInvoiceExportColumn> columns,
        DateTime exportedAt,
        string? language = null);
}
