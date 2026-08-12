using System.ComponentModel.DataAnnotations;

namespace Shabakat.Application.DTOs.DistributionBox;

public record DistributionBoxResponse(
    Guid Id,
    string Name,
    Guid AreaId,
    string AreaName,
    string? LocationNote,
    string? Notes,
    int CustomerCount,
    DateTime CreatedAt);

public record CreateDistributionBoxRequest(
    [Required][MaxLength(200)] string Name,
    [Required] Guid AreaId,
    [MaxLength(500)] string? LocationNote = null,
    [MaxLength(1000)] string? Notes = null);

public record UpdateDistributionBoxRequest(
    [Required][MaxLength(200)] string Name,
    [Required] Guid AreaId,
    [MaxLength(500)] string? LocationNote = null,
    [MaxLength(1000)] string? Notes = null);
