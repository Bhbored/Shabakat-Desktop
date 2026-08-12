using Shabakat.Domain.Common;
using Shabakat.Domain.Enums;

namespace Shabakat.Domain.Entities;

public class Payment : Base
{
    public Guid CustomerId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
    public Customer Customer { get; set; } = null!;
    public Invoice Invoice { get; set; } = null!;
}
