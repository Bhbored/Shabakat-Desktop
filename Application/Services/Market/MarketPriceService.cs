using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Market;
using Shabakat.Application.Options;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Market;

public sealed class MarketPriceService : IMarketPriceService
{
    private const int MaxResponseCharacters = 3_000_000;
    private const string GlobalClientName = "GlobalMarket";
    private const string LebanonClientName = "LebanonMarket";
    private static readonly SemaphoreSlim CacheGate = new(1, 1);
    private static readonly JsonSerializerOptions CacheJsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly (string Code, string Symbol, string Unit)[] Instruments =
    [
        ("WTI", "CL=F", "USD/barrel"),
        ("BRENT", "BZ=F", "USD/barrel"),
        ("NATURAL_GAS", "NG=F", "USD/MMBtu"),
        ("RBOB", "RB=F", "USD/gallon"),
        ("HEATING_OIL", "HO=F", "USD/gallon")
    ];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMarketConnectivity _connectivity;
    private readonly MarketDataOptions _options;
    private readonly ILogger<MarketPriceService> _logger;
    private MarketCacheFile? _cache;

    public MarketPriceService(
        IHttpClientFactory httpClientFactory,
        IMarketConnectivity connectivity,
        IOptions<MarketDataOptions> options,
        ILogger<MarketPriceService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _connectivity = connectivity;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GlobalMarketResponse> GetGlobalAsync(
        MarketRange range,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        await CacheGate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCacheLoadedAsync(cancellationToken);
            EnsureConnected();
            var key = range.ToString();
            if (!forceRefresh
                && _cache!.Global.TryGetValue(key, out var cached)
                && DateTimeOffset.UtcNow - cached.FetchedAt < TimeSpan.FromMinutes(Math.Max(1, _options.GlobalCacheMinutes)))
                return cached;

            try
            {
                var client = _httpClientFactory.CreateClient(GlobalClientName);
                var quotes = await FetchYahooQuotesAsync(client, range, cancellationToken);
                var response = new GlobalMarketResponse(range, quotes, DateTimeOffset.UtcNow);
                _cache!.Global[key] = response;
                await SaveCacheAsync(cancellationToken);
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Global market data fetch failed");
                throw new DomainException("Error.Market.GlobalUnavailable", ex);
            }
        }
        finally
        {
            CacheGate.Release();
        }
    }

    public async Task<LebanonMarketResponse> GetLebanonAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        await CacheGate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCacheLoadedAsync(cancellationToken);
            EnsureConnected();
            if (!forceRefresh
                && _cache!.Lebanon is { } cached
                && DateTimeOffset.UtcNow - cached.FetchedAt < TimeSpan.FromMinutes(Math.Max(1, _options.LebanonCacheMinutes)))
                return cached;

            try
            {
                var client = _httpClientFactory.CreateClient(LebanonClientName);
                var html = await GetStringWithRetryAsync(client, _options.IptFuelPricesUrl, cancellationToken);
                var response = ParseIpt(html) with { FetchedAt = DateTimeOffset.UtcNow };
                _cache!.Lebanon = response;
                await SaveCacheAsync(cancellationToken);
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
        finally
        {
            CacheGate.Release();
        }
    }

    public async Task<GlobalMarketResponse?> GetCachedGlobalAsync(
        MarketRange range,
        CancellationToken cancellationToken = default)
    {
        await CacheGate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCacheLoadedAsync(cancellationToken);
            return _cache!.Global.GetValueOrDefault(range.ToString());
        }
        finally
        {
            CacheGate.Release();
        }
    }

    public async Task<LebanonMarketResponse?> GetCachedLebanonAsync(
        CancellationToken cancellationToken = default)
    {
        await CacheGate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCacheLoadedAsync(cancellationToken);
            return _cache!.Lebanon;
        }
        finally
        {
            CacheGate.Release();
        }
    }

    private void EnsureConnected()
    {
        if (!_connectivity.HasInternetAccess)
            throw new DomainException("Error.Market.NoConnection");
    }

    private async Task<IReadOnlyList<CommodityQuoteResponse>> FetchYahooQuotesAsync(
        HttpClient client,
        MarketRange range,
        CancellationToken cancellationToken)
    {
        var symbols = Uri.EscapeDataString(string.Join(',', Instruments.Select(instrument => instrument.Symbol)));
        var path = $"v7/finance/spark?symbols={symbols}&range={YahooRange(range)}&interval=1d";
        var json = await GetStringWithRetryAsync(client, path, cancellationToken);

        try
        {
            using var document = JsonDocument.Parse(json);
            var results = document.RootElement.GetProperty("spark").GetProperty("result");
            var responsesBySymbol = results.EnumerateArray().ToDictionary(
                item => item.GetProperty("symbol").GetString() ?? string.Empty,
                item => item.GetProperty("response")[0]);

            return Instruments.Select(instrument =>
            {
                if (!responsesBySymbol.TryGetValue(instrument.Symbol, out var result))
                    throw new InvalidDataException($"Yahoo response omitted {instrument.Symbol}");
                return ParseYahooQuote(result, instrument);
            }).ToList();
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException or KeyNotFoundException or InvalidOperationException or FormatException or InvalidDataException)
        {
            _logger.LogWarning(ex, "Invalid Yahoo multi-symbol response");
            throw new DomainException("Error.Market.InvalidResponse", ex);
        }
    }

    private static CommodityQuoteResponse ParseYahooQuote(
        JsonElement result,
        (string Code, string Symbol, string Unit) instrument)
    {
        var meta = result.GetProperty("meta");
        var timestamps = result.GetProperty("timestamp");
        var closes = result.GetProperty("indicators").GetProperty("quote")[0].GetProperty("close");
        var points = new List<MarketPricePointResponse>();
        var count = Math.Min(timestamps.GetArrayLength(), closes.GetArrayLength());
        for (var index = 0; index < count; index++)
        {
            if (closes[index].ValueKind == JsonValueKind.Null)
                continue;
            points.Add(new MarketPricePointResponse(
                DateTimeOffset.FromUnixTimeSeconds(timestamps[index].GetInt64()),
                closes[index].GetDecimal()));
        }

        if (points.Count == 0)
            throw new InvalidDataException("Yahoo response contained no price points");

        return new CommodityQuoteResponse(
            instrument.Code,
            instrument.Symbol,
            meta.GetProperty("regularMarketPrice").GetDecimal(),
            ReadDecimal(meta, "regularMarketChange"),
            ReadDecimal(meta, "regularMarketChangePercent"),
            meta.GetProperty("currency").GetString() ?? "USD",
            instrument.Unit,
            DateTimeOffset.FromUnixTimeSeconds(meta.GetProperty("regularMarketTime").GetInt64()),
            points);
    }

    private static decimal ReadDecimal(JsonElement parent, string name) =>
        parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDecimal()
            : 0;

    private static LebanonMarketResponse ParseIpt(string html)
    {
        var quotes = new[]
        {
            ParseIptQuote(html, "tr95Octane", "UNL95"),
            ParseIptQuote(html, "tr98Octane", "UNL98"),
            ParseIptQuote(html, "trDiesel", "DIESEL"),
            ParseIptQuote(html, "trGas", "LPG")
        };

        var chartMatch = Regex.Match(
            html,
            "categories:\\s*\\[(?<dates>.*?)\\]\\},valueAxis",
            RegexOptions.Singleline | RegexOptions.CultureInvariant);
        var seriesMatch = Regex.Match(
            html,
            "_series\":\"(?<series>.*?)\",\"_uniqueId\":\"[^\"]*MyStatsChart\"",
            RegexOptions.Singleline | RegexOptions.CultureInvariant);
        if (!chartMatch.Success || !seriesMatch.Success)
            throw new InvalidDataException("IPT history was not found");

        var dates = Regex.Matches(chartMatch.Groups["dates"].Value, @"new Date\((\d{4}),\s*(\d{1,2}),\s*(\d{1,2})")
            .Select(m => new DateTimeOffset(
                int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
                int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture) + 1,
                int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture),
                0, 0, 0, TimeSpan.Zero))
            .ToList();
        var decodedSeries = Regex.Unescape(WebUtility.HtmlDecode(seriesMatch.Groups["series"].Value));
        var series = new[]
        {
            ParseIptSeries(decodedSeries, dates, "98 Octane", "UNL98"),
            ParseIptSeries(decodedSeries, dates, "95 Octane", "UNL95"),
            ParseIptSeries(decodedSeries, dates, "Diesel", "DIESEL"),
            ParseIptSeries(decodedSeries, dates, "Gas", "LPG")
        };

        if (dates.Count == 0)
            throw new InvalidDataException("IPT history contained no dates");

        return new LebanonMarketResponse(quotes, series, dates[^1], DateTimeOffset.UtcNow);
    }

    private static LebanonFuelQuoteResponse ParseIptQuote(string html, string rowSuffix, string code)
    {
        var match = Regex.Match(
            html,
            $"<tr[^>]+id=\"[^\"]*{Regex.Escape(rowSuffix)}\"[\\s\\S]*?<strong[^>]*>(?<price>[^<]+)</strong>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success)
            throw new InvalidDataException($"IPT quote {code} was not found");

        var digits = Regex.Replace(WebUtility.HtmlDecode(match.Groups["price"].Value), @"[^0-9]", "");
        if (!decimal.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var price) || price <= 0)
            throw new InvalidDataException($"IPT quote {code} was invalid");
        return new LebanonFuelQuoteResponse(code, price, "L.L.");
    }

    private static LebanonFuelSeriesResponse ParseIptSeries(
        string source,
        IReadOnlyList<DateTimeOffset> dates,
        string sourceName,
        string code)
    {
        var match = Regex.Match(
            source,
            $"name:\\s*'{Regex.Escape(sourceName)}'.*?data:\\s*\\[(?<values>.*?)\\]",
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success)
            throw new InvalidDataException($"IPT series {code} was not found");

        var values = Regex.Matches(match.Groups["values"].Value, @"value:\s*([0-9]+(?:\.[0-9]+)?)")
            .Select(m => decimal.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
            .ToList();
        if (values.Count != dates.Count)
            throw new InvalidDataException($"IPT series {code} did not match its dates");

        return new LebanonFuelSeriesResponse(
            code,
            dates.Select((date, index) => new MarketPricePointResponse(date, values[index])).ToList());
    }

    private static string YahooRange(MarketRange range) => range switch
    {
        MarketRange.ThreeMonths => "3mo",
        MarketRange.OneYear => "1y",
        _ => "1mo"
    };

    private static async Task<string> GetStringWithRetryAsync(
        HttpClient client,
        string requestUri,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    if (attempt == 0 && ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500))
                    {
                        await Task.Delay(500, cancellationToken);
                        continue;
                    }
                    throw new HttpRequestException($"Market source returned HTTP {(int)response.StatusCode}");
                }

                if (response.Content.Headers.ContentLength > MaxResponseCharacters)
                    throw new InvalidDataException("Market response was too large");
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (content.Length > MaxResponseCharacters)
                    throw new InvalidDataException("Market response was too large");
                return content;
            }
            catch (HttpRequestException) when (attempt == 0)
            {
                await Task.Delay(500, cancellationToken);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attempt == 0)
            {
                await Task.Delay(500, cancellationToken);
            }
        }
        throw new HttpRequestException("Market source request failed");
    }

    private string CachePath => Path.Combine(FileSystem.AppDataDirectory, "market-prices-cache-v1.json");

    private async Task EnsureCacheLoadedAsync(CancellationToken cancellationToken)
    {
        if (_cache is not null)
            return;
        try
        {
            if (File.Exists(CachePath))
            {
                var json = await File.ReadAllTextAsync(CachePath, cancellationToken);
                _cache = JsonSerializer.Deserialize<MarketCacheFile>(json, CacheJsonOptions);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _logger.LogWarning(ex, "Ignoring invalid market price cache");
        }
        _cache ??= new MarketCacheFile();
    }

    private async Task SaveCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            var path = CachePath;
            var temporaryPath = path + ".tmp";
            var json = JsonSerializer.Serialize(_cache, CacheJsonOptions);
            await File.WriteAllTextAsync(temporaryPath, json, cancellationToken);
            File.Move(temporaryPath, path, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Market price cache could not be saved");
        }
    }

    private sealed class MarketCacheFile
    {
        public Dictionary<string, GlobalMarketResponse> Global { get; set; } = [];
        public LebanonMarketResponse? Lebanon { get; set; }
    }
}
