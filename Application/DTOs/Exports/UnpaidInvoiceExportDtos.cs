using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Exports;

public sealed record UnpaidInvoiceExportRow(
    int InvoiceNumber,
    DateOnly ConsumptionStart,
    DateOnly ConsumptionEnd,
    DateOnly PaymentDueDate,
    string InvoiceStatus,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal AmountDue,
    decimal? BilledConsumption,
    decimal FixedCharge,
    decimal TVA,
    string CustomerName,
    string? CustomerPhone,
    string? Address,
    string? Building,
    string? Floor,
    string? CableName,
    string? AreaName,
    string? BoxName,
    string? AmpereScheduleName,
    CustomerType CustomerType,
    PlanType Plan,
    decimal PlanValue,
    CustomerStatus CustomerStatus,
    CustomerRelation? CustomerRelation);

public sealed record UnpaidInvoiceExportFile(
    byte[] Content,
    string FileName,
    string ContentType);
