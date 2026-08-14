namespace Shabakat.Application.DTOs.Profile;

public sealed record ProfileResponse(
    Guid Id,
    string? BusinessName,
    string? LogoUrl);

public sealed record UpdateProfileRequest(
    string? BusinessName,
    string? LogoUrl);
