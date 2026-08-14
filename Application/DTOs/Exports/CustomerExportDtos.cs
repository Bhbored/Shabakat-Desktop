using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Exports;

public sealed record CustomerExportRequest(
    IReadOnlyCollection<Guid>? AreaIds = null,
    IReadOnlyCollection<CustomerExportColumn>? Columns = null,
    CustomerExportScope Scope = CustomerExportScope.Full);

public sealed record ExportAreaRef(Guid Id, string Name);

public sealed record CustomerExportRow(
    string Name,
    string? Phone,
    string? Address,
    string? Building,
    string? Floor,
    string? CableName,
    string? AreaName,
    string? BoxName,
    string? AmpereScheduleName,
    string CustomerType,
    PlanType Plan,
    decimal PlanValue,
    DateOnly SubscriptionDate,
    string CustomerStatus,
    string? CustomerRelation,
    decimal? InitialMeterReading,
    decimal? LatestMeterReading,
    decimal TotalBilled,
    decimal TotalPaid,
    decimal TotalToPay);

public sealed record CustomerExportPlanGroup(
    PlanType Plan,
    IReadOnlyList<CustomerExportRow> Customers);

public sealed record CustomerExportGroup(
    string BoxName,
    IReadOnlyList<CustomerExportPlanGroup> Plans)
{
    public int CustomerCount => Plans.Sum(p => p.Customers.Count);

    public IEnumerable<CustomerExportRow> AllCustomers => Plans.SelectMany(p => p.Customers);
}

public sealed record CustomerExportSheet(
    string AreaName,
    IReadOnlyList<CustomerExportGroup> Groups)
{
    public int CustomerCount => Groups.Sum(g => g.CustomerCount);
}

public sealed record ExportBoxRow(
    string Name,
    string? LocationNote,
    string? Notes,
    int CustomerCount);

public sealed record BoxStructureSheet(
    string SheetName,
    string AreaName,
    ExportBoxRow Box);

public sealed record CustomerExportBoxSheet(
    string SheetName,
    string AreaName,
    string BoxName,
    IReadOnlyList<CustomerExportPlanGroup> Plans)
{
    public int CustomerCount => Plans.Sum(p => p.Customers.Count);
}

public sealed record AreaStructureSheet(
    string AreaName,
    IReadOnlyList<ExportBoxRow> Boxes);

public sealed record CustomerExportFile(
    byte[] Content,
    string FileName,
    string ContentType);
