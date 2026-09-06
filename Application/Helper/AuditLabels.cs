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
        var customerName = Detail(entry, "Customer Name", "CustomerName");
        var invoiceNumber = Detail(entry, "Invoice Number", "InvoiceNumber");
        var expenseType = DetailValue(entry, "Expense Type", localizer);
        if (string.IsNullOrWhiteSpace(expenseType))
            expenseType = DetailValue(entry, "ExpenseType", localizer);
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
            nameof(AuditAction.InvoicePaymentRecorded) => localizer["AuditSummary.InvoicePaymentRecorded", amount,
                invoiceNumber],
            nameof(AuditAction.InvoiceFixedKilowattCharge) => localizer["AuditSummary.InvoiceFixedKilowattCharge",
                customerName, invoiceNumber],
            _ => null
        };

        if (formatted is not { } localized || localized.ResourceNotFound)
            return entry.Summary;

        return localized.Value;
    }

    public static string DetailLabel(string label, IStringLocalizer<SharedResource> localizer)
    {
        var key = NormalizeLabel(label) switch
        {
            "name" => "Common.Name",
            "plan" => "Common.Plan",
            "phone" => "Common.Phone",
            "customertype" => "Field.CustomerType",
            "expensetype" => "Field.ExpenseType",
            "amount" => "Common.Amount",
            "expensedate" => "Table.ExpenseDate",
            "label" => "Table.Label",
            "invoicenumber" => "Table.InvoiceNumber",
            "customername" or "customer" => "Common.Customer",
            "totalamount" => "Invoices.TotalAmount",
            "consumptionstart" => "Invoices.ConsumptionStart",
            "consumptionend" => "Invoices.ConsumptionEnd",
            "created" => "Invoices.BulkCreated",
            "skipped" => "Invoices.BulkSkipped",
            "paymentmethod" => "Invoices.PaymentMethod",
            "paidamount" => "Invoices.PaidAmount",
            "paymentamount" => "Invoices.PaymentAmount",
            "billedconsumption" => "Invoices.BilledConsumption",
            _ => null
        };

        if (key is null)
            return label;

        var localized = localizer[key];
        return localized.ResourceNotFound ? label : localized.Value;
    }

    public static string DetailValue(AuditLogResponse entry, string label,
        IStringLocalizer<SharedResource> localizer) =>
        DetailValue(label, Detail(entry, label), localizer);

    public static string DetailValue(string label, string value, IStringLocalizer<SharedResource> localizer)
    {
        if (string.IsNullOrWhiteSpace(value))
            return localizer["Common.Dash"].Value;

        var resourceKey = NormalizeLabel(label) switch
        {
            "plan" => $"PlanType.{value}",
            "customertype" => $"CustomerType.{value}",
            "expensetype" => $"ExpenseType.{value}",
            "paymentmethod" => $"PaymentMethod.{value}",
            _ => null
        } ?? TryEnumResourceKey(value);

        if (resourceKey is null)
            return value;

        var localized = localizer[resourceKey];
        return localized.ResourceNotFound ? value : localized.Value;
    }

    private static string? TryEnumResourceKey(string value)
    {
        if (Enum.TryParse<PlanType>(value, ignoreCase: true, out _))
            return $"PlanType.{value}";
        if (Enum.TryParse<CustomerType>(value, ignoreCase: true, out _))
            return $"CustomerType.{value}";
        if (Enum.TryParse<ExpenseType>(value, ignoreCase: true, out _))
            return $"ExpenseType.{value}";
        if (Enum.TryParse<PaymentMethod>(value, ignoreCase: true, out _))
            return $"PaymentMethod.{value}";
        return null;
    }

    private static string Detail(AuditLogResponse entry, params string[] labels)
    {
        foreach (var label in labels)
        {
            var match = entry.Details.FirstOrDefault(d =>
                NormalizeLabel(d.Label) == NormalizeLabel(label));
            if (match is not null)
                return match.Value;
        }

        return string.Empty;
    }

    private static string NormalizeLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return string.Empty;

        return string.Concat(label.Where(char.IsLetterOrDigit)).ToLowerInvariant();
    }
}
