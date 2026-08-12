using Shabakat.Domain.Enums;

namespace Shabakat.Components.Features.Subscribers.Components;

public sealed class SubscriberFormModel
{
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string Building { get; set; } = "";
    public string Floor { get; set; } = "";
    public string CableName { get; set; } = "";
    public string AreaId { get; set; } = "";
    public string BoxId { get; set; } = "";
    public string AmpereScheduleId { get; set; } = "";
    public CustomerType CustomerType { get; set; } = CustomerType.Residential;
    public PlanType Plan { get; set; } = PlanType.Ampere;
    public decimal PlanValue { get; set; } = 5;
    public DateTime? SubscriptionDate { get; set; }
    public string CustomerRelation { get; set; } = "";
    public CustomerStatus CustomerStatus { get; set; } = CustomerStatus.Active;
    public decimal? InitialMeterReading { get; set; }
    public bool UsePricingOverride { get; set; }
    public bool HadPricingOverride { get; set; }
    public decimal? OverridePrice { get; set; }
    public decimal? OverrideFixedCharge { get; set; }
    public decimal? OverrideTva { get; set; }
}
