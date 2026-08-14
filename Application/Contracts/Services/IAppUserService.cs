using Shabakat.Application.DTOs.Profile;

namespace Shabakat.Application.Contracts.Services;

public interface IAppUserService
{
    event Action? Changed;

    Task<ProfileResponse?> GetAsync();
    Task<ProfileResponse> UpsertAsync(UpdateProfileRequest request);
}
