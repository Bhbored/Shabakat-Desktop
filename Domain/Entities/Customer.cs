using Shabakat.Domain.Common;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Domain.Entities;

public class Customer : Base
{
    public Customer()
    {
        Invoices = new HashSet<Invoice>();
        Payments = new HashSet<Payment>();
        MeterReadings = new HashSet<MeterReading>();
    }

    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? CableName { get; set; }
    public Guid? BoxId { get; set; }
    public Guid? AmpereScheduleId { get; set; }
    public CustomerType CustomerType { get; set; }
    public CustomerRelation? CustomerRelation { get; set; }
    public DateOnly SubscriptionDate { get; set; }

    public decimal? PriceOverride { get; set; }
    public decimal? FixedChargeOverride { get; set; }
    public decimal? TVAOverride { get; set; }

    public bool HasPricingOverride => PriceOverride is not null;

    public void SetPricingOverride(decimal price, decimal fixedCharge, decimal tva)
    {
            if (price < 0)
                throw new DomainException("Error.PriceOverrideNegative");
            if (fixedCharge < 0)
                throw new DomainException("Error.FixedChargeOverrideNegative");
            if (tva < 0 || tva > 100)
                throw new DomainException("Error.TvaOverrideRange");

        PriceOverride = price;
        FixedChargeOverride = fixedCharge;
        TVAOverride = tva;
    }

    public void ClearPricingOverride()
    {
        PriceOverride = null;
        FixedChargeOverride = null;
        TVAOverride = null;
    }

    public CustomerStatus CustomerStatus { get; set; } = CustomerStatus.Active;
    public PlanType Plan { get; set; }
    public decimal PlanValue { get; set; }
    public Guid? AreaId { get; set; }

    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<MeterReading> MeterReadings { get; set; } = [];
    public Area? Area { get; set; }
    public DistributionBox? DistributionBox { get; set; }
    public AmpereSchedule? AmpereSchedule { get; set; }
}
