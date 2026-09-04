using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Market;
using Shabakat.Application.Helper;
using Shabakat.Application.Options;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Market;

public sealed class GlobalMarketService : IGlobalMarketService
{
    private const string ClientName = "GlobalMarket";
    private static readonly (string Code, string Symbol, string Unit)[] Instruments =
    [
        ("WTI", "CL=F", "USD/barrel"),
        ("BRENT", "BZ=F", "USD/barrel"),
        ("NATURAL_GAS", "NG=F", "USD/MMBtu"),
        ("RBOB", "RB=F", "USD/gallon"),
        ("HEATING_OIL", "HO=F", "USD/gallon")
    ];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MarketDataHelper _helper;
    private readonly MarketDataOptions _options;
    private readonly ILogger<GlobalMarketService> _logger;

    public GlobalMarketService(
        IHttpClientFactory httpClientFactory,
        MarketDataHelper helper,
        IOptions<MarketDataOptions> options,
        ILogger<GlobalMarketService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _helper = helper;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GlobalMarketResponse> GetAsync(
        MarketRange range,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        _helper.EnsureConnected();
        var cached = await _helper.GetCachedGlobalAsync(range, cancellationToken);
        if (!forceRefresh && cached is not null && IsFresh(cached))
            return cached;

        try
        {
            var client = _httpClientFactory.CreateClient(ClientName);
            var symbols = Uri.EscapeDataString(string.Join(',', Instruments.Select(item => item.Symbol)));
            var path = $"v7/finance/spark?symbols={symbols}&range={ToYahooRange(range)}&interval=1d";
            var json = await _helper.GetResponseAsync(client, path, cancellationToken);
            var quotes = _helper.ParseYahooQuotes(json, Instruments);
            var response = new GlobalMarketResponse(range, quotes, DateTimeOffset.UtcNow);
            await _helper.StoreGlobalAsync(response, cancellationToken);
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
            _logger.LogWarning(ex, "Yahoo returned an unexpected response");
            throw new DomainException("Error.Market.InvalidResponse", ex);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Global market data fetch failed");
            throw new DomainException("Error.Market.GlobalUnavailable", ex);
        }
    }

    public Task<GlobalMarketResponse?> GetCachedAsync(
        MarketRange range,
        CancellationToken cancellationToken = default) =>
        _helper.GetCachedGlobalAsync(range, cancellationToken);

    private bool IsFresh(GlobalMarketResponse response) =>
        DateTimeOffset.UtcNow - response.FetchedAt
        < TimeSpan.FromMinutes(Math.Max(1, _options.GlobalCacheMinutes));

    private static string ToYahooRange(MarketRange range) => range switch
    {
        MarketRange.ThreeMonths => "3mo",
        MarketRange.OneYear => "1y",
        _ => "1mo"
    };
}
