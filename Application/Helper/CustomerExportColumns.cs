using Shabakat.Domain.Enums;

namespace Shabakat.Application.Helper;

public static class CustomerExportColumns
{
    public static readonly IReadOnlyList<CustomerExportColumn> All =
        Enum.GetValues<CustomerExportColumn>();

    public static readonly IReadOnlyList<CustomerExportColumn> Default =
    [
        CustomerExportColumn.Name,
        CustomerExportColumn.Phone,
        CustomerExportColumn.Floor,
        CustomerExportColumn.PlanValue,
        CustomerExportColumn.TotalToPay
    ];

    private static readonly HashSet<CustomerExportColumn> Money =
    [
        CustomerExportColumn.PlanValue,
        CustomerExportColumn.TotalBilled,
        CustomerExportColumn.TotalPaid,
        CustomerExportColumn.TotalToPay
    ];

    public static bool IsMoney(CustomerExportColumn column) => Money.Contains(column);

    public static string Header(CustomerExportColumn column) => column switch
    {
        CustomerExportColumn.Name => "Name",
        CustomerExportColumn.Phone => "Phone",
        CustomerExportColumn.Address => "Address",
        CustomerExportColumn.Building => "Building",
        CustomerExportColumn.Floor => "Floor",
        CustomerExportColumn.CableName => "Cable Name",
        CustomerExportColumn.AreaName => "Area",
        CustomerExportColumn.BoxName => "Box",
        CustomerExportColumn.AmpereScheduleName => "Ampere Schedule",
        CustomerExportColumn.CustomerType => "Customer Type",
        CustomerExportColumn.Plan => "Plan",
        CustomerExportColumn.PlanValue => "Plan Value",
        CustomerExportColumn.SubscriptionDate => "Subscription Date",
        CustomerExportColumn.CustomerStatus => "Status",
        CustomerExportColumn.CustomerRelation => "Relation",
        CustomerExportColumn.InitialMeterReading => "Initial Reading",
        CustomerExportColumn.LatestMeterReading => "Latest Reading",
        CustomerExportColumn.TotalBilled => "Total Billed",
        CustomerExportColumn.TotalPaid => "Total Paid",
        CustomerExportColumn.TotalToPay => "Total To Pay",
        _ => column.ToString()
    };

    public static IReadOnlyList<CustomerExportColumn> Resolve(
        IReadOnlyCollection<CustomerExportColumn>? requested)
    {
        if (requested is null || requested.Count == 0)
            return Default;

        var resolved = requested.Distinct().ToList();
        return resolved.Count == 0 ? Default : resolved;
    }
}
