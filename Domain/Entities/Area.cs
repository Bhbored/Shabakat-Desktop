using Shabakat.Domain.Common;

namespace Shabakat.Domain.Entities;

public class Area : Base
{
    public Area()
    {
        Customers = new HashSet<Customer>();
        DistributionBoxes = new HashSet<DistributionBox>();
    }

    public string Name { get; set; } = string.Empty;

    public ICollection<Customer> Customers { get; set; }
    public ICollection<DistributionBox> DistributionBoxes { get; set; }
}
