using Shabakat.Domain.Common;

namespace Shabakat.Domain.Entities;

public class MeterReading : Base
{
    public Guid CustomerId { get; set; }
    public decimal ReadingValue { get; set; }
    public DateOnly ReadingDate { get; set; }
    public bool IsInitial { get; set; }

    public Customer Customer { get; set; } = null!;
}
