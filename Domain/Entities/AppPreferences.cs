using Shabakat.Domain.Common;

namespace Shabakat.Domain.Entities;

public class AppPreferences : Base
{
    public decimal PricePerKilowat { get; set; }
    public decimal PricePerAmp { get; set; }
    public decimal FixedCharge { get; set; }
    public decimal TVA { get; set; }

    public decimal ResidentialPricePerAmp { get; set; }
    public decimal ResidentialPricePerKilowat { get; set; }
    public decimal ResidentialFixedCharge { get; set; }
    public decimal ResidentialTVA { get; set; }

    public decimal CommercialPricePerAmp { get; set; }
    public decimal CommercialPricePerKilowat { get; set; }
    public decimal CommercialFixedCharge { get; set; }
    public decimal CommercialTVA { get; set; }

    public decimal IndustrialPricePerAmp { get; set; }
    public decimal IndustrialPricePerKilowat { get; set; }
    public decimal IndustrialFixedCharge { get; set; }
    public decimal IndustrialTVA { get; set; }

    public bool AmpereSchedulePricingEnabled { get; set; }
    public bool AmpereProrateByDaysEnabled { get; set; }

    public string Language { get; set; } = "en";
    public int DueDate { get; set; } = 31;

    public CustomerExportColumnPreference? CustomerExportColumnPreference { get; set; }

    public CustomerExportColumnPreference EnsureExportColumns()
        => CustomerExportColumnPreference ??= new CustomerExportColumnPreference();
}
