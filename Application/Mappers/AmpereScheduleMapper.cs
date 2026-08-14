using Shabakat.Application.DTOs.AmpereSchedule;

namespace Shabakat.Application.Mappers;

public static class AmpereScheduleMapper
{
    public static AmpereScheduleResponse ToResponse(this Domain.Entities.AmpereSchedule schedule)
    {
        var customerCount = schedule.Customers?.Count ?? 0;
        return new(
            Id: schedule.Id,
            Name: schedule.Name,
            HoursPerDay: schedule.HoursPerDay,
            PricePerAmp: schedule.PricePerAmp,
            ResidentialPricePerAmp: schedule.ResidentialPricePerAmp,
            CommercialPricePerAmp: schedule.CommercialPricePerAmp,
            IndustrialPricePerAmp: schedule.IndustrialPricePerAmp,
            CustomerCount: customerCount,
            CanBeDeleted: customerCount == 0,
            CreatedAt: schedule.CreatedAt);
    }
}
