using Shabakat.Domain.Common;

namespace Shabakat.Domain.Entities;

public class InvoiceSkip : Base
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly BillingPeriodStart { get; set; }
    public DateOnly BillingPeriodEnd { get; set; }
    public string Reason { get; set; } = string.Empty;

    public Customer Customer { get; set; } = null!;
}
