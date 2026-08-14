using Shabakat.Application.DTOs.Area;

namespace Shabakat.Application.Mappers;

public static class AreaMapper
{
    public static AreaResponse ToResponse(this Domain.Entities.Area a) =>
        new(
            Id: a.Id,
            Name: a.Name,
            CustomerCount: a.Customers?.Count ?? 0,
            CreatedAt: a.CreatedAt);
}
