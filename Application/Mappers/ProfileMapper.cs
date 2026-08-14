using Shabakat.Application.DTOs.Profile;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Mappers;

public static class ProfileMapper
{
    public static ProfileResponse ToResponse(this AppUser user) =>
        new(
            Id: user.Id,
            FullName: user.FullName,
            Username: user.Username,
            BusinessName: user.BusinessName,
            LogoUrl: user.LogoUrl);
}
