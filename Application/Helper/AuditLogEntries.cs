using System.Text.Json;
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
            EntityType: AuditEntityTypes.Customer,
            EntityId: customer.Id,
            Details: Format(new Dictionary<string, object?>
            {
                ["name"] = customer.Name,
                ["plan"] = customer.Plan.ToString(),
                ["phone"] = customer.Phone,
                ["customerType"] = customer.CustomerType.ToString()
            }));

    public static AuditLogWriteRequest ExpenseCreated(Expenses expense) =>
        new(
            Action: AuditAction.ExpenseCreated,
            Summary: $"Expense recorded ({expense.ExpenseType})",
            EntityType: AuditEntityTypes.Expense,
            EntityId: expense.Id,
            Details: Format(new Dictionary<string, object?>
            {
                ["expenseType"] = expense.ExpenseType.ToString(),
                ["amount"] = expense.Amount,
                ["expenseDate"] = expense.ExpenseDate,
                ["label"] = expense.Label
            }));

    public static AuditLogWriteRequest InvoiceCreated(
        Invoice invoice,
        string customerName,
        string plan) =>
        new(
            Action: AuditAction.InvoiceCreated,
            Summary: $"Invoice #{invoice.InvoiceNumber} created for {customerName}",
            EntityType: AuditEntityTypes.Invoice,
            EntityId: invoice.Id,
            Details: Format(new Dictionary<string, object?>
            {
                ["invoiceNumber"] = invoice.InvoiceNumber,
                ["customerName"] = customerName,
                ["plan"] = plan,
                ["totalAmount"] = invoice.TotalAmount,
                ["consumptionStart"] = invoice.IssueDate,
                ["consumptionEnd"] = invoice.DueDate
            }));

    public static AuditLogWriteRequest InvoiceBulkCreated(int created, int skipped) =>
        new(
            Action: AuditAction.InvoiceBulkCreated,
            Summary: $"Bulk invoices: {created} created, {skipped} skipped",
            EntityType: AuditEntityTypes.Invoice,
            Details: Format(new Dictionary<string, object?>
            {
                ["created"] = created,
                ["skipped"] = skipped
            }));

    public static AuditLogWriteRequest InvoicePaymentRecorded(
        Invoice invoice,
        decimal amount,
        PaymentMethod method) =>
        new(
            Action: AuditAction.InvoicePaymentRecorded,
            Summary: $"Payment of {amount:F4} recorded on invoice #{invoice.InvoiceNumber}",
            EntityType: AuditEntityTypes.Payment,
            EntityId: invoice.Id,
            Details: Format(new Dictionary<string, object?>
            {
                ["invoiceNumber"] = invoice.InvoiceNumber,
                ["amount"] = amount,
                ["paymentMethod"] = method.ToString(),
                ["totalAmount"] = invoice.TotalAmount,
                ["paidAmount"] = invoice.PaidAmount
            }));

    public static AuditLogWriteRequest InvoiceFixedKilowattCharge(
        Invoice invoice,
        string customerName,
        decimal paymentAmount,
        decimal billedConsumption) =>
        new(
            Action: AuditAction.InvoiceFixedKilowattCharge,
            Summary: $"Fixed kilowatt charge for {customerName} (invoice #{invoice.InvoiceNumber})",
            EntityType: AuditEntityTypes.Invoice,
            EntityId: invoice.Id,
            Details: Format(new Dictionary<string, object?>
            {
                ["invoiceNumber"] = invoice.InvoiceNumber,
                ["customerName"] = customerName,
                ["paymentAmount"] = paymentAmount,
                ["billedConsumption"] = billedConsumption,
                ["totalAmount"] = invoice.TotalAmount
            }));

    private static string? Format(IReadOnlyDictionary<string, object?> parameters)
    {
        if (parameters.Count == 0)
            return null;

        return JsonSerializer.Serialize(parameters);
    }
}
