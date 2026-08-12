namespace Shabakat.Application.DTOs.Payment;

public record PaymentResponse(
    Guid Id,
    Guid InvoiceId,
    string CustomerName,
    Guid CustomerId,
    decimal Amount,
    string PaymentMethod,
    DateTime PaymentDate,
    string? Notes,
    DateTime CreatedAt);
