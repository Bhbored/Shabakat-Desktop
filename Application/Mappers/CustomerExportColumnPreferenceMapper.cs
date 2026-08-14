using Shabakat.Application.Helper;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Mappers;

public static class CustomerExportColumnPreferenceMapper
{
    public static IReadOnlyList<CustomerExportColumn> ToSelectedColumns(
        this CustomerExportColumnPreference prefs)
    {
        var selected = new List<CustomerExportColumn>();

        if (prefs.Name) selected.Add(CustomerExportColumn.Name);
        if (prefs.Phone) selected.Add(CustomerExportColumn.Phone);
        if (prefs.Address) selected.Add(CustomerExportColumn.Address);
        if (prefs.Building) selected.Add(CustomerExportColumn.Building);
        if (prefs.Floor) selected.Add(CustomerExportColumn.Floor);
        if (prefs.CableName) selected.Add(CustomerExportColumn.CableName);
        if (prefs.AreaName) selected.Add(CustomerExportColumn.AreaName);
        if (prefs.BoxName) selected.Add(CustomerExportColumn.BoxName);
        if (prefs.AmpereScheduleName) selected.Add(CustomerExportColumn.AmpereScheduleName);
        if (prefs.CustomerType) selected.Add(CustomerExportColumn.CustomerType);
        if (prefs.Plan) selected.Add(CustomerExportColumn.Plan);
        if (prefs.PlanValue) selected.Add(CustomerExportColumn.PlanValue);
        if (prefs.SubscriptionDate) selected.Add(CustomerExportColumn.SubscriptionDate);
        if (prefs.CustomerStatus) selected.Add(CustomerExportColumn.CustomerStatus);
        if (prefs.CustomerRelation) selected.Add(CustomerExportColumn.CustomerRelation);
        if (prefs.InitialMeterReading) selected.Add(CustomerExportColumn.InitialMeterReading);
        if (prefs.LatestMeterReading) selected.Add(CustomerExportColumn.LatestMeterReading);
        if (prefs.TotalBilled) selected.Add(CustomerExportColumn.TotalBilled);
        if (prefs.TotalPaid) selected.Add(CustomerExportColumn.TotalPaid);
        if (prefs.TotalToPay) selected.Add(CustomerExportColumn.TotalToPay);

        return CustomerExportColumns.Resolve(selected);
    }

    public static CustomerExportColumnPreference Apply(
        this CustomerExportColumnPreference prefs,
        IReadOnlyCollection<CustomerExportColumn> selected)
    {
        var set = selected as HashSet<CustomerExportColumn> ?? selected.ToHashSet();

        prefs.Name = set.Contains(CustomerExportColumn.Name);
        prefs.Phone = set.Contains(CustomerExportColumn.Phone);
        prefs.Address = set.Contains(CustomerExportColumn.Address);
        prefs.Building = set.Contains(CustomerExportColumn.Building);
        prefs.Floor = set.Contains(CustomerExportColumn.Floor);
        prefs.CableName = set.Contains(CustomerExportColumn.CableName);
        prefs.AreaName = set.Contains(CustomerExportColumn.AreaName);
        prefs.BoxName = set.Contains(CustomerExportColumn.BoxName);
        prefs.AmpereScheduleName = set.Contains(CustomerExportColumn.AmpereScheduleName);
        prefs.CustomerType = set.Contains(CustomerExportColumn.CustomerType);
        prefs.Plan = set.Contains(CustomerExportColumn.Plan);
        prefs.PlanValue = set.Contains(CustomerExportColumn.PlanValue);
        prefs.SubscriptionDate = set.Contains(CustomerExportColumn.SubscriptionDate);
        prefs.CustomerStatus = set.Contains(CustomerExportColumn.CustomerStatus);
        prefs.CustomerRelation = set.Contains(CustomerExportColumn.CustomerRelation);
        prefs.InitialMeterReading = set.Contains(CustomerExportColumn.InitialMeterReading);
        prefs.LatestMeterReading = set.Contains(CustomerExportColumn.LatestMeterReading);
        prefs.TotalBilled = set.Contains(CustomerExportColumn.TotalBilled);
        prefs.TotalPaid = set.Contains(CustomerExportColumn.TotalPaid);
        prefs.TotalToPay = set.Contains(CustomerExportColumn.TotalToPay);

        return prefs;
    }
}
