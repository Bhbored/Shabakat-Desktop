using System.ComponentModel.DataAnnotations;
using Shabakat.Application.DTOs.Payment;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Invoices;

public record InvoiceResponse(
    Guid Id,
    int InvoiceNumber,
    string CustomerName,
    string? CustomerPhone,
    Guid? CustomerId,
    DateOnly ConsumptionStart,
    DateOnly ConsumptionEnd,
    DateOnly PaymentDueDate,
    decimal FixedCharge,
    decimal TVA,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal AmountDue,
    decimal? BilledConsumption,
    string InvoiceStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<PaymentResponse> Payments);

public record InvoiceSummaryResponse(
    Guid Id,
    int InvoiceNumber,
    string CustomerName,
    string InvoiceStatus,
    DateOnly ConsumptionStart,
    DateOnly ConsumptionEnd,
    DateOnly PaymentDueDate,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal AmountDue,
    decimal? BilledConsumption,
    DateTime CreatedAt,
    bool CanBeDeleted);

public record CreateInvoiceRequest(
    [Required] Guid CustomerId,
    [Range(1, 31)] int? BilledDays = null,
    [Range(0.0001, double.MaxValue)] decimal? PaymentAmount = null,
    [Range(0.0001, double.MaxValue)] decimal? KilowattAmount = null,
    PaymentMethod? PaymentMethod = null,
    string? Notes = null);

public record BulkCreateInvoiceResponse(
    int Created,
    int Skipped,
    string Message);

public record InvoiceSkippedResponse(
    Guid CustomerId,
    string CustomerName,
    string Reason,
    DateTime SkippedAt);

public record AddPaymentRequest(
    [Required][Range(0.0001, double.MaxValue)] decimal Amount,
    [Required] PaymentMethod PaymentMethod,
    string? Notes = null);

public record UpdateInvoiceRequest(
    DateOnly? ConsumptionStart = null,
    DateOnly? ConsumptionEnd = null);

public record FixedKilowattCalculateRequest(
    [Required] CustomerType CustomerType,
    [Range(0, 9999999)] decimal? PlanValue = null,
    [Range(0.0001, double.MaxValue)] decimal? PaymentAmount = null,
    [Range(0.0001, double.MaxValue)] decimal? KilowattAmount = null);

public record FixedKilowattCalculateResponse(
    decimal PaymentAmount,
    decimal KilowattAmount,
    decimal UnitPrice,
    decimal FixedCharge,
    decimal TVA,
    decimal PlanValue,
    string CustomerType);
