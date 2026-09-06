using Shabakat.Domain.Enums;

namespace Shabakat.Application.Helper;

public static class UnpaidInvoiceExportColumns
{
    private static readonly UnpaidInvoiceExportColumn[] AmountsAfterCustomerName =
    [
        UnpaidInvoiceExportColumn.TotalAmount,
        UnpaidInvoiceExportColumn.PaidAmount,
        UnpaidInvoiceExportColumn.AmountDue
    ];

    public static readonly IReadOnlyList<UnpaidInvoiceExportColumn> InvoiceColumns =
    [
        UnpaidInvoiceExportColumn.ConsumptionStart,
        UnpaidInvoiceExportColumn.ConsumptionEnd,
        UnpaidInvoiceExportColumn.PaymentDueDate,
        UnpaidInvoiceExportColumn.InvoiceStatus,
        UnpaidInvoiceExportColumn.BilledConsumption,
        UnpaidInvoiceExportColumn.FixedCharge,
        UnpaidInvoiceExportColumn.TVA
    ];

    public static string Header(UnpaidInvoiceExportColumn column) => column switch
    {
        UnpaidInvoiceExportColumn.InvoiceNumber => "Invoice #",
        UnpaidInvoiceExportColumn.ConsumptionStart => "Consumption Start",
        UnpaidInvoiceExportColumn.ConsumptionEnd => "Consumption End",
        UnpaidInvoiceExportColumn.PaymentDueDate => "Payment Due Date",
        UnpaidInvoiceExportColumn.InvoiceStatus => "Invoice Status",
        UnpaidInvoiceExportColumn.TotalAmount => "Invoice Total",
        UnpaidInvoiceExportColumn.PaidAmount => "Paid Amount",
        UnpaidInvoiceExportColumn.AmountDue => "Amount Due",
        UnpaidInvoiceExportColumn.BilledConsumption => "Billed Consumption",
        UnpaidInvoiceExportColumn.FixedCharge => "Fixed Charge",
        UnpaidInvoiceExportColumn.TVA => "TVA",
        UnpaidInvoiceExportColumn.CustomerName => "Customer Name",
        UnpaidInvoiceExportColumn.CustomerPhone => "Phone",
        UnpaidInvoiceExportColumn.Address => "Address",
        UnpaidInvoiceExportColumn.Building => "Building",
        UnpaidInvoiceExportColumn.Floor => "Floor",
        UnpaidInvoiceExportColumn.CableName => "Cable Name",
        UnpaidInvoiceExportColumn.AreaName => "Area",
        UnpaidInvoiceExportColumn.BoxName => "Box",
        UnpaidInvoiceExportColumn.AmpereScheduleName => "Ampere Schedule",
        UnpaidInvoiceExportColumn.CustomerType => "Customer Type",
        UnpaidInvoiceExportColumn.Plan => "Plan",
        UnpaidInvoiceExportColumn.PlanValue => "Plan Value",
        UnpaidInvoiceExportColumn.CustomerStatus => "Customer Status",
        UnpaidInvoiceExportColumn.CustomerRelation => "Customer Relation",
        _ => column.ToString()
    };

    public static IReadOnlyList<UnpaidInvoiceExportColumn> Resolve(
        IReadOnlyCollection<CustomerExportColumn>? customerColumns)
    {
        var selected = CustomerExportColumns.Resolve(customerColumns);
        var columns = new List<UnpaidInvoiceExportColumn> { UnpaidInvoiceExportColumn.InvoiceNumber };
        var amountsInserted = false;

        foreach (var customerColumn in CustomerExportColumns.All)
        {
            if (!selected.Contains(customerColumn))
                continue;

            var mapped = MapCustomerColumn(customerColumn);
            if (!mapped.HasValue)
                continue;

            columns.Add(mapped.Value);
            if (mapped.Value == UnpaidInvoiceExportColumn.CustomerName)
            {
                columns.AddRange(AmountsAfterCustomerName);
                amountsInserted = true;
            }
        }

        if (!amountsInserted)
            columns.AddRange(AmountsAfterCustomerName);

        columns.AddRange(InvoiceColumns);
        return columns;
    }

    private static UnpaidInvoiceExportColumn? MapCustomerColumn(CustomerExportColumn column) => column switch
    {
        CustomerExportColumn.Name => UnpaidInvoiceExportColumn.CustomerName,
        CustomerExportColumn.Phone => UnpaidInvoiceExportColumn.CustomerPhone,
        CustomerExportColumn.Address => UnpaidInvoiceExportColumn.Address,
        CustomerExportColumn.Building => UnpaidInvoiceExportColumn.Building,
        CustomerExportColumn.Floor => UnpaidInvoiceExportColumn.Floor,
        CustomerExportColumn.CableName => UnpaidInvoiceExportColumn.CableName,
        CustomerExportColumn.AreaName => UnpaidInvoiceExportColumn.AreaName,
        CustomerExportColumn.BoxName => UnpaidInvoiceExportColumn.BoxName,
        CustomerExportColumn.AmpereScheduleName => UnpaidInvoiceExportColumn.AmpereScheduleName,
        CustomerExportColumn.CustomerType => UnpaidInvoiceExportColumn.CustomerType,
        CustomerExportColumn.Plan => UnpaidInvoiceExportColumn.Plan,
        CustomerExportColumn.PlanValue => UnpaidInvoiceExportColumn.PlanValue,
        CustomerExportColumn.CustomerStatus => UnpaidInvoiceExportColumn.CustomerStatus,
        CustomerExportColumn.CustomerRelation => UnpaidInvoiceExportColumn.CustomerRelation,
        _ => null
    };
}
