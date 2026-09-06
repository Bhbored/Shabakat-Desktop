using System.ComponentModel.DataAnnotations;

namespace Shabakat.Application.DTOs.Area;

public record AreaResponse(
    Guid Id,
    string Name,
    int CustomerCount,
    int BoxCount,
    DateTime CreatedAt);

public record CreateAreaRequest(
    [Required][MaxLength(200)] string Name);

public record UpdateAreaRequest(
    [Required][MaxLength(200)] string Name);
