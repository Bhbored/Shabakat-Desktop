using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Market;
using Shabakat.Application.Helper;
using Shabakat.Application.Options;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Market;

public sealed class LebanonMarketService : ILebanonMarketService
{
    private const string ClientName = "LebanonMarket";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MarketDataHelper _helper;
    private readonly MarketDataOptions _options;
    private readonly ILogger<LebanonMarketService> _logger;

    public LebanonMarketService(
        IHttpClientFactory httpClientFactory,
        MarketDataHelper helper,
        IOptions<MarketDataOptions> options,
        ILogger<LebanonMarketService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _helper = helper;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<LebanonMarketResponse> GetAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        _helper.EnsureConnected();
        var cached = await _helper.GetCachedLebanonAsync(cancellationToken);
        if (!forceRefresh && cached is not null && IsFresh(cached))
            return cached;

        try
        {
            var client = _httpClientFactory.CreateClient(ClientName);
            var html = await _helper.GetResponseAsync(
                client,
                _options.IptFuelPricesUrl,
                cancellationToken);
            var response = _helper.ParseIpt(html) with { FetchedAt = DateTimeOffset.UtcNow };
            await _helper.StoreLebanonAsync(response, cancellationToken);
            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (InvalidDataException ex)
        {
            _logger.LogWarning(ex, "IPT returned an unexpected response");
            throw new DomainException("Error.Market.InvalidResponse", ex);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lebanon fuel data fetch failed");
            throw new DomainException("Error.Market.LebanonUnavailable", ex);
        }
    }

    public Task<LebanonMarketResponse?> GetCachedAsync(
        CancellationToken cancellationToken = default) =>
        _helper.GetCachedLebanonAsync(cancellationToken);

    private bool IsFresh(LebanonMarketResponse response) =>
        DateTimeOffset.UtcNow - response.FetchedAt
        < TimeSpan.FromMinutes(Math.Max(1, _options.LebanonCacheMinutes));
}
