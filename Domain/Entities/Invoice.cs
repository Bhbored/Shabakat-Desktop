using Shabakat.Domain.Common;
using Shabakat.Domain.Enums;

namespace Shabakat.Domain.Entities;

public class Invoice : Base
{
    public Invoice()
    {
        Payments = new HashSet<Payment>();
    }

    public Guid CustomerId { get; set; }
    public int InvoiceNumber { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal FixedCharge { get; set; }
    public decimal TVA { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal? BilledConsumption { get; set; }
    public decimal AmountDue { get; set; }
    public InvoiceStatus InvoiceStatus { get; set; } = InvoiceStatus.Unpaid;

    public Customer Customer { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = [];
}
