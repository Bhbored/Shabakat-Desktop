using Shabakat.Application.DTOs.Preferences;

namespace Shabakat.Application.Contracts.Services;

public interface IAppPreferencesService
{
    Task<GetPreferencesResponse?> GetAsync();
    Task UpsertAsync(UpdatePreferencesRequest request);
}
