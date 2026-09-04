using Shabakat.Application.DTOs.Market;

namespace Shabakat.Application.Contracts.Services;

public interface ILebanonMarketService
{
    Task<LebanonMarketResponse> GetAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);

    Task<LebanonMarketResponse?> GetCachedAsync(
        CancellationToken cancellationToken = default);
}
