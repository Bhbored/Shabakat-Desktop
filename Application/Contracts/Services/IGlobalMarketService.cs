using Shabakat.Application.DTOs.Market;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Contracts.Services;

public interface IGlobalMarketService
{
    Task<GlobalMarketResponse> GetAsync(
        MarketRange range,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);

    Task<GlobalMarketResponse?> GetCachedAsync(
        MarketRange range,
        CancellationToken cancellationToken = default);
}
