using Shabakat.Application.DTOs.DistributionBox;

namespace Shabakat.Application.Mappers;

public static class DistributionBoxMapper
{
    public static DistributionBoxResponse ToResponse(this Domain.Entities.DistributionBox box)
    {
        var customerCount = box.Customers?.Count ?? 0;
        return new(
            Id: box.Id,
            Name: box.Name,
            AreaId: box.AreaId,
            AreaName: box.Area?.Name ?? string.Empty,
            LocationNote: box.LocationNote,
            Notes: box.Notes,
            CustomerCount: customerCount,
            CanBeDeleted: customerCount == 0,
            CreatedAt: box.CreatedAt);
    }
}
