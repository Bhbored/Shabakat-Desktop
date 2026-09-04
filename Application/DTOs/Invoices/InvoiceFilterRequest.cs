using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Invoices;

public enum InvoiceDueTimingFilter
{
    Overdue,
    DueToday,
    DueSoon
}

public record InvoiceFilterRequest(
    Guid? CustomerId = null,
    InvoiceStatus? InvoiceStatus = null,
    DateOnly? ConsumptionStartFrom = null,
    DateOnly? ConsumptionStartTo = null,
    InvoiceDueTimingFilter? DueTiming = null,
    int PageNumber = 1,
    int PageSize = 10);
