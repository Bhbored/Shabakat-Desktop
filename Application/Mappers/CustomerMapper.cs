using Shabakat.Application.DTOs.Customers;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Mappers;

public static class CustomerMapper
{
    public static CustomerSummaryResponse ToSummary(this Customer c)
    {
        var amountDue = c.Invoices
            .Where(i => i.InvoiceStatus != InvoiceStatus.Paid)
            .Sum(i => i.AmountDue);

        return new CustomerSummaryResponse(
            Id: c.Id,
            Name: c.Name,
            Phone: c.Phone,
            Address: c.Address,
            Building: c.Building,
            Floor: c.Floor,
            CableName: c.CableName,
            BoxId: c.BoxId,
            BoxName: c.DistributionBox?.Name,
            AmpereScheduleId: c.AmpereScheduleId,
            AmpereScheduleName: c.AmpereSchedule?.Name,
            CustomerType: c.CustomerType.ToString(),
            Plan: c.Plan.ToString(),
            AreaName: c.Area?.Name,
            PlanValue: c.PlanValue,
            CustomerStatus: c.CustomerStatus.ToString(),
            SubscriptionDate: c.SubscriptionDate,
            CreatedAt: c.CreatedAt,
            HasPricingOverride: c.HasPricingOverride,
            CustomerRelation: c.CustomerRelation?.ToString(),
            AmountDue: amountDue,
            CanBeDeleted: !c.Invoices.Any());
    }

    public static CustomerResponse ToResponse(this Customer c)
    {
        return new CustomerResponse(
            Id: c.Id,
            Name: c.Name,
            Phone: c.Phone,
            AreaName: c.Area?.Name,
            Address: c.Address,
            Building: c.Building,
            Floor: c.Floor,
            CableName: c.CableName,
            BoxId: c.BoxId,
            BoxName: c.DistributionBox?.Name,
            AmpereScheduleId: c.AmpereScheduleId,
            AmpereScheduleName: c.AmpereSchedule?.Name,
            CustomerType: c.CustomerType.ToString(),
            Plan: c.Plan.ToString(),
            PlanValue: c.PlanValue,
            InitialMeterReading: c.MeterReadings
                .FirstOrDefault(m => m.IsInitial)?.ReadingValue,
            CustomerStatus: c.CustomerStatus.ToString(),
            SubscriptionDate: c.SubscriptionDate,
            CreatedAt: c.CreatedAt,
            CustomerRelation: c.CustomerRelation?.ToString(),
            HasPricingOverride: c.HasPricingOverride,
            PricingOverride: c.HasPricingOverride
                ? new CustomerPricingOverrideDto(
                    c.PriceOverride!.Value,
                    c.FixedChargeOverride!.Value,
                    c.TVAOverride!.Value)
                : null,
            TotalBilled: c.Invoices.Sum(i => i.TotalAmount),
            TotalPaid: c.Invoices.Sum(i => i.PaidAmount),
            TotalOutstanding: c.Invoices.Sum(i => i.AmountDue),
            PaidThisMonth: c.HasPaidThisMonth());
    }

    private static bool HasPaidThisMonth(this Customer customer)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        return customer.Invoices.Any(i =>
            i.IssueDate >= monthStart &&
            i.IssueDate < monthEnd &&
            i.InvoiceStatus == InvoiceStatus.Paid);
    }
}
