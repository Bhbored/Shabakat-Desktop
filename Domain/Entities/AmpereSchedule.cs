using Shabakat.Domain.Common;

namespace Shabakat.Domain.Entities;

public class AmpereSchedule : Base
{
    public AmpereSchedule()
    {
        Customers = new HashSet<Customer>();
    }

    public string Name { get; set; } = string.Empty;
    public int HoursPerDay { get; set; }
    public decimal PricePerAmp { get; set; }
    public decimal ResidentialPricePerAmp { get; set; }
    public decimal CommercialPricePerAmp { get; set; }
    public decimal IndustrialPricePerAmp { get; set; }

    public ICollection<Customer> Customers { get; set; } = [];
}
