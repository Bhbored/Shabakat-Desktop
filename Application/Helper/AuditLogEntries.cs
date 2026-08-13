using System.Globalization;
using Shabakat.Application.DTOs.AuditLogs;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Helper;

public static class AuditLogEntries
{
    public static AuditLogWriteRequest CustomerCreated(Customer customer) =>
        new(
            Action: AuditAction.CustomerCreated,
            Summary: $"Customer '{customer.Name}' created",
            EntityType: AuditEntityType.Customer,
            EntityId: customer.Id,
            Details: Items(
                ("Name", customer.Name),
                ("Plan", customer.Plan.ToString()),
                ("Phone", customer.Phone),
                ("Customer Type", customer.CustomerType.ToString())));

    public static AuditLogWriteRequest ExpenseCreated(Expenses expense) =>
        new(
            Action: AuditAction.ExpenseCreated,
            Summary: $"Expense recorded ({expense.ExpenseType})",
            EntityType: AuditEntityType.Expense,
            EntityId: expense.Id,
            Details: Items(
                ("Expense Type", expense.ExpenseType.ToString()),
                ("Amount", Text(expense.Amount)),
                ("Expense Date", Text(expense.ExpenseDate)),
                ("Label", expense.Label)));

    public static AuditLogWriteRequest InvoiceCreated(
        Invoice invoice,
        string customerName,
        string plan) =>
        new(
            Action: AuditAction.InvoiceCreated,
            Summary: $"Invoice #{invoice.InvoiceNumber} created for {customerName}",
            EntityType: AuditEntityType.Invoice,
            EntityId: invoice.Id,
            Details: Items(
                ("Invoice Number", Text(invoice.InvoiceNumber)),
                ("Customer Name", customerName),
                ("Plan", plan),
                ("Total Amount", Text(invoice.TotalAmount)),
                ("Consumption Start", Text(invoice.IssueDate)),
                ("Consumption End", Text(invoice.DueDate))));

    public static AuditLogWriteRequest InvoiceBulkCreated(int created, int skipped) =>
        new(
            Action: AuditAction.InvoiceBulkCreated,
            Summary: $"Bulk invoices: {created} created, {skipped} skipped",
            EntityType: AuditEntityType.Invoice,
            Details: Items(
                ("Created", Text(created)),
                ("Skipped", Text(skipped))));

    public static AuditLogWriteRequest InvoicePaymentRecorded(
        Invoice invoice,
        decimal amount,
        PaymentMethod method) =>
        new(
            Action: AuditAction.InvoicePaymentRecorded,
            Summary: $"Payment of {amount:F4} recorded on invoice #{invoice.InvoiceNumber}",
            EntityType: AuditEntityType.Payment,
            EntityId: invoice.Id,
            Details: Items(
                ("Invoice Number", Text(invoice.InvoiceNumber)),
                ("Amount", Text(amount)),
                ("Payment Method", method.ToString()),
                ("Total Amount", Text(invoice.TotalAmount)),
                ("Paid Amount", Text(invoice.PaidAmount))));

    public static AuditLogWriteRequest InvoiceFixedKilowattCharge(
        Invoice invoice,
        string customerName,
        decimal paymentAmount,
        decimal billedConsumption) =>
        new(
            Action: AuditAction.InvoiceFixedKilowattCharge,
            Summary: $"Fixed kilowatt charge for {customerName} (invoice #{invoice.InvoiceNumber})",
            EntityType: AuditEntityType.Invoice,
            EntityId: invoice.Id,
            Details: Items(
                ("Invoice Number", Text(invoice.InvoiceNumber)),
                ("Customer Name", customerName),
                ("Payment Amount", Text(paymentAmount)),
                ("Billed Consumption", Text(billedConsumption)),
                ("Total Amount", Text(invoice.TotalAmount))));

    private static IReadOnlyList<AuditLogDetailItem> Items(params (string Label, string? Value)[] entries) =>
        entries
            .Where(e => !string.IsNullOrWhiteSpace(e.Value))
            .Select(e => new AuditLogDetailItem(e.Label, e.Value!))
            .ToList();

    private static string Text(decimal value) =>
        value.ToString("0.####", CultureInfo.InvariantCulture);

    private static string Text(int value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private static string Text(DateOnly value) =>
        value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
