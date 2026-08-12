using Shabakat.Domain.Common;

namespace Shabakat.Domain.Entities;

public class DistributionBox : Base
{
    public DistributionBox()
    {
        Customers = new HashSet<Customer>();
    }

    public string Name { get; set; } = string.Empty;
    public Guid AreaId { get; set; }
    public string? LocationNote { get; set; }
    public string? Notes { get; set; }

    public Area Area { get; set; } = null!;
    public ICollection<Customer> Customers { get; set; } = [];
}
