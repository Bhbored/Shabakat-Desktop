namespace Shabakat.Application.DTOs.DistributionBox;

public record DistributionBoxFilterRequest(
    Guid? AreaId = null,
    string? Name = null,
    int PageNumber = 1,
    int PageSize = 10);
