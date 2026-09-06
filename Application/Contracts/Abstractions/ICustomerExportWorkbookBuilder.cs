using Shabakat.Application.DTOs.Exports;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Contracts.Abstractions;

public interface ICustomerExportWorkbookBuilder
{
    ICustomerExportWorkbook Create(
        IReadOnlyList<CustomerExportColumn> columns,
        DateTime exportedAt,
        string? language = null);
}

public interface ICustomerExportWorkbook : IDisposable
{
    void AddSheet(CustomerExportSheet sheet);
    void AddFlatSheet(string sheetName, IReadOnlyList<CustomerExportRow> rows);
    void AddStructureSheet(AreaStructureSheet sheet);
    void AddBoxSheet(CustomerExportBoxSheet sheet);
    void AddBoxStructureSheet(BoxStructureSheet sheet);
    byte[] ToBytes();
}
