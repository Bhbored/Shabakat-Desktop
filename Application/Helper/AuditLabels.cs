using Microsoft.Extensions.Localization;
using Shabakat.Application.DTOs.AuditLogs;
using Shabakat.Domain.Enums;
using Shabakat.Resources.Localization;

namespace Shabakat.Application.Helper;

public static class AuditLabels
{
    public static string Action(AuditAction action, IStringLocalizer<SharedResource> localizer) =>
        Action(action.ToString(), localizer);

    public static string Action(string? action, IStringLocalizer<SharedResource> localizer)
    {
        if (string.IsNullOrWhiteSpace(action))
            return localizer["Common.Dash"].Value;

        var localized = localizer[$"AuditAction.{action}"];
        return localized.ResourceNotFound ? action : localized.Value;
    }

    public static string EntityType(string? entityType, IStringLocalizer<SharedResource> localizer)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return localizer["Common.Dash"].Value;

        var localized = localizer[$"AuditEntityType.{entityType}"];
        return localized.ResourceNotFound ? entityType : localized.Value;
    }

    public static string Summary(AuditLogResponse entry, IStringLocalizer<SharedResource> localizer)
    {
        var name = Detail(entry, "Name");
        var customerName = Detail(entry, "Customer Name");
        var invoiceNumber = Detail(entry, "Invoice Number");
        var expenseType = DetailValue(entry, "Expense Type", localizer);
        var created = Detail(entry, "Created");
        var skipped = Detail(entry, "Skipped");
        var amount = Detail(entry, "Amount");

        LocalizedString? formatted = entry.Action switch
        {
            nameof(AuditAction.CustomerCreated) => localizer["AuditSummary.CustomerCreated", name],
            nameof(AuditAction.CustomerUpdated) => localizer["AuditSummary.CustomerUpdated", name],
            nameof(AuditAction.CustomerDeleted) => localizer["AuditSummary.CustomerDeleted", name],
            nameof(AuditAction.ExpenseCreated) => localizer["AuditSummary.ExpenseCreated", expenseType],
            nameof(AuditAction.ExpenseUpdated) => localizer["AuditSummary.ExpenseUpdated", expenseType],
            nameof(AuditAction.ExpenseDeleted) => localizer["AuditSummary.ExpenseDeleted", expenseType],
            nameof(AuditAction.InvoiceCreated) => localizer["AuditSummary.InvoiceCreated", invoiceNumber, customerName],
            nameof(AuditAction.InvoiceBulkCreated) => localizer["AuditSummary.InvoiceBulkCreated", created, skipped],
            nameof(AuditAction.InvoicePaymentRecorded) => localizer["AuditSummary.InvoicePaymentRecorded", amount, invoiceNumber],
            nameof(AuditAction.InvoiceFixedKilowattCharge) => localizer["AuditSummary.InvoiceFixedKilowattCharge", customerName, invoiceNumber],
            _ => null
        };

        if (formatted is not { } localized || localized.ResourceNotFound)
            return entry.Summary;

        return localized.Value;
    }

    public static string DetailLabel(string label, IStringLocalizer<SharedResource> localizer)
    {
        var key = label switch
        {
            "Name" => "Common.Name",
            "Plan" => "Common.Plan",
            "Phone" => "Common.Phone",
            "Customer Type" => "Field.CustomerType",
            "Expense Type" => "Field.ExpenseType",
            "Amount" => "Common.Amount",
            "Expense Date" => "Table.ExpenseDate",
            "Label" => "Table.Label",
            "Invoice Number" => "Table.InvoiceNumber",
            "Customer Name" => "Common.Customer",
            "Total Amount" => "Invoices.TotalAmount",
            "Consumption Start" => "Invoices.ConsumptionStart",
            "Consumption End" => "Invoices.ConsumptionEnd",
            "Created" => "Invoices.BulkCreated",
            "Skipped" => "Invoices.BulkSkipped",
            "Payment Method" => "Invoices.PaymentMethod",
            "Paid Amount" => "Invoices.PaidAmount",
            "Payment Amount" => "Invoices.PaymentAmount",
            "Billed Consumption" => "Invoices.BilledConsumption",
            _ => null
        };

        if (key is null)
            return label;

        var localized = localizer[key];
        return localized.ResourceNotFound ? label : localized.Value;
    }

    public static string DetailValue(AuditLogResponse entry, string label, IStringLocalizer<SharedResource> localizer) =>
        DetailValue(label, Detail(entry, label), localizer);

    public static string DetailValue(string label, string value, IStringLocalizer<SharedResource> localizer)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var key = label switch
        {
            "Plan" => $"PlanType.{value}",
            "Customer Type" => $"CustomerType.{value}",
            "Expense Type" => $"ExpenseType.{value}",
            "Payment Method" => $"PaymentMethod.{value}",
            _ => null
        };

        if (key is null)
            return value;

        var localized = localizer[key];
        return localized.ResourceNotFound ? value : localized.Value;
    }

    private static string Detail(AuditLogResponse entry, string label) =>
        entry.Details.FirstOrDefault(d => d.Label.Equals(label, StringComparison.OrdinalIgnoreCase))?.Value
        ?? string.Empty;
}
