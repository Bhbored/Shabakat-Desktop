namespace Shabakat.Application.DTOs.Profile;

public sealed record ProfileResponse(
    Guid Id,
    string FullName,
    string Username,
    string? BusinessName,
    string? LogoUrl);

public sealed record UpdateProfileRequest(
    string FullName,
    string Username,
    string? BusinessName,
    string? LogoUrl);
