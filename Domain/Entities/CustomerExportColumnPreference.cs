using Shabakat.Domain.Common;

namespace Shabakat.Domain.Entities;

public class CustomerExportColumnPreference : Base
{
    public Guid AppPreferencesId { get; set; }

    public bool Name { get; set; } = true;
    public bool Phone { get; set; } = true;
    public bool Address { get; set; }
    public bool Building { get; set; }
    public bool Floor { get; set; } = true;
    public bool CableName { get; set; }
    public bool AreaName { get; set; }
    public bool BoxName { get; set; }
    public bool AmpereScheduleName { get; set; }
    public bool CustomerType { get; set; }
    public bool Plan { get; set; }
    public bool PlanValue { get; set; } = true;
    public bool SubscriptionDate { get; set; }
    public bool CustomerStatus { get; set; }
    public bool CustomerRelation { get; set; }
    public bool InitialMeterReading { get; set; }
    public bool LatestMeterReading { get; set; }
    public bool TotalBilled { get; set; }
    public bool TotalPaid { get; set; }
    public bool TotalToPay { get; set; } = true;
}
