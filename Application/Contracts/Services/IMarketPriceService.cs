using Shabakat.Application.DTOs.Market;

namespace Shabakat.Application.Contracts.Services;

public interface IMarketPriceService
{
    Task<GlobalMarketResponse> GetGlobalAsync(
        MarketRange range,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);

    Task<LebanonMarketResponse> GetLebanonAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);

    Task<GlobalMarketResponse?> GetCachedGlobalAsync(
        MarketRange range,
        CancellationToken cancellationToken = default);

    Task<LebanonMarketResponse?> GetCachedLebanonAsync(
        CancellationToken cancellationToken = default);
}

public interface IMarketConnectivity
{
    bool HasInternetAccess { get; }
    event EventHandler<bool>? ConnectivityChanged;
}
