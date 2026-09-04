using Shabakat.Application.DTOs.Invoices;
using Shabakat.Application.DTOs.Payment;
using Shabakat.Application.Helper;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Mappers;

public static class InvoiceMapper
{
    public static InvoiceSummaryResponse ToSummary(this Invoice i, int paymentDueDay) =>
        new(
            Id: i.Id,
            InvoiceNumber: i.InvoiceNumber,
            CustomerName: i.Customer?.Name ?? string.Empty,
            InvoiceStatus: i.InvoiceStatus.ToString(),
            ConsumptionStart: i.IssueDate,
            ConsumptionEnd: i.DueDate,
            PaymentDueDate: BillingPeriodHelper.ResolvePaymentDueDate(
                DateOnly.FromDateTime(i.CreatedAt), paymentDueDay),
            TotalAmount: i.TotalAmount,
            PaidAmount: i.PaidAmount,
            AmountDue: i.AmountDue,
            BilledConsumption: i.BilledConsumption,
            CreatedAt: i.CreatedAt,
            CanBeDeleted: i.InvoiceStatus == InvoiceStatus.Unpaid);

    public static InvoiceResponse ToResponse(this Invoice i, int paymentDueDay) =>
        new(
            Id: i.Id,
            InvoiceNumber: i.InvoiceNumber,
            CustomerName: i.Customer?.Name ?? string.Empty,
            CustomerPhone: i.Customer?.Phone,
            CustomerId: i.CustomerId,
            InvoiceStatus: i.InvoiceStatus.ToString(),
            ConsumptionStart: i.IssueDate,
            ConsumptionEnd: i.DueDate,
            PaymentDueDate: BillingPeriodHelper.ResolvePaymentDueDate(
                DateOnly.FromDateTime(i.CreatedAt), paymentDueDay),
            FixedCharge: i.FixedCharge,
            TVA: i.TVA,
            TotalAmount: i.TotalAmount,
            PaidAmount: i.PaidAmount,
            AmountDue: i.AmountDue,
            BilledConsumption: i.BilledConsumption,
            CreatedAt: i.CreatedAt,
            UpdatedAt: i.UpdatedAt,
            Payments: i.Payments.Select(p => p.ToResponse()));

    public static PaymentResponse ToResponse(this Payment p) =>
        new(
            Id: p.Id,
            InvoiceId: p.InvoiceId,
            CustomerName: p.Customer?.Name ?? string.Empty,
            CustomerId: p.CustomerId,
            Amount: p.Amount,
            PaymentMethod: p.PaymentMethod.ToString(),
            PaymentDate: p.PaymentDate,
            Notes: p.Notes,
            CreatedAt: p.CreatedAt);
}
