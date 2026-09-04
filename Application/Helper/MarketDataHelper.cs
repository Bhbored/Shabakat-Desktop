using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Market;
using Shabakat.Application.Models.Market;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Helper;

public sealed class MarketDataHelper
{
    private const int MaxResponseCharacters = 3_000_000;
    private static readonly JsonSerializerOptions CacheJsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _cacheGate = new(1, 1);
    private readonly IMarketConnectivity _connectivity;
    private readonly ILogger<MarketDataHelper> _logger;
    private MarketCacheFile? _cache;

    public MarketDataHelper(
        IMarketConnectivity connectivity,
        ILogger<MarketDataHelper> logger)
    {
        _connectivity = connectivity;
        _logger = logger;
    }

    public void EnsureConnected()
    {
        if (!_connectivity.HasInternetAccess)
            throw new DomainException("Error.Market.NoConnection");
    }

    public async Task<string> GetResponseAsync(
        HttpClient client,
        string requestUri,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                using var response = await client.GetAsync(
                    requestUri,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                ValidateResponse(response, attempt);

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

    public IReadOnlyList<CommodityQuoteResponse> ParseYahooQuotes(
        string json,
        IReadOnlyList<(string Code, string Symbol, string Unit)> instruments)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var results = document.RootElement.GetProperty("spark").GetProperty("result");
            var responsesBySymbol = results.EnumerateArray().ToDictionary(
                item => item.GetProperty("symbol").GetString() ?? string.Empty,
                item => item.GetProperty("response")[0]);

            return instruments.Select(instrument =>
            {
                if (!responsesBySymbol.TryGetValue(instrument.Symbol, out var result))
                    throw new InvalidDataException($"Yahoo response omitted {instrument.Symbol}");
                return ParseYahooQuote(result, instrument);
            }).ToList();
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException or KeyNotFoundException
                                   or InvalidOperationException or FormatException or InvalidDataException)
        {
            throw new InvalidDataException("Yahoo returned an invalid market response", ex);
        }
    }

    public LebanonMarketResponse ParseIpt(string html)
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

        var dates = Regex.Matches(
                chartMatch.Groups["dates"].Value,
                @"new Date\((\d{4}),\s*(\d{1,2}),\s*(\d{1,2})")
            .Select(match => new DateTimeOffset(
                int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) + 1,
                int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
                0, 0, 0, TimeSpan.Zero))
            .ToList();
        if (dates.Count == 0)
            throw new InvalidDataException("IPT history contained no dates");

        var decodedSeries = Regex.Unescape(WebUtility.HtmlDecode(seriesMatch.Groups["series"].Value));
        var series = new[]
        {
            ParseIptSeries(decodedSeries, dates, "98 Octane", "UNL98"),
            ParseIptSeries(decodedSeries, dates, "95 Octane", "UNL95"),
            ParseIptSeries(decodedSeries, dates, "Diesel", "DIESEL"),
            ParseIptSeries(decodedSeries, dates, "Gas", "LPG")
        };

        return new LebanonMarketResponse(quotes, series, dates[^1], DateTimeOffset.UtcNow);
    }

    public async Task<GlobalMarketResponse?> GetCachedGlobalAsync(
        MarketRange range,
        CancellationToken cancellationToken)
    {
        await _cacheGate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCacheLoadedAsync(cancellationToken);
            return _cache!.Global.GetValueOrDefault(range.ToString());
        }
        finally
        {
            _cacheGate.Release();
        }
    }

    public async Task<LebanonMarketResponse?> GetCachedLebanonAsync(CancellationToken cancellationToken)
    {
        await _cacheGate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCacheLoadedAsync(cancellationToken);
            return _cache!.Lebanon;
        }
        finally
        {
            _cacheGate.Release();
        }
    }

    public async Task StoreGlobalAsync(
        GlobalMarketResponse response,
        CancellationToken cancellationToken)
    {
        await _cacheGate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCacheLoadedAsync(cancellationToken);
            _cache!.Global[response.Range.ToString()] = response;
            await SaveCacheAsync(cancellationToken);
        }
        finally
        {
            _cacheGate.Release();
        }
    }

    public async Task StoreLebanonAsync(
        LebanonMarketResponse response,
        CancellationToken cancellationToken)
    {
        await _cacheGate.WaitAsync(cancellationToken);
        try
        {
            await EnsureCacheLoadedAsync(cancellationToken);
            _cache!.Lebanon = response;
            await SaveCacheAsync(cancellationToken);
        }
        finally
        {
            _cacheGate.Release();
        }
    }

    private static void ValidateResponse(HttpResponseMessage response, int attempt)
    {
        if (!response.IsSuccessStatusCode)
        {
            if (attempt == 0 && ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500))
                throw new HttpRequestException("Market source request should be retried");
            throw new HttpRequestException($"Market source returned HTTP {(int)response.StatusCode}");
        }

        if (response.Content.Headers.ContentLength > MaxResponseCharacters)
            throw new InvalidDataException("Market response was too large");
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
            .Select(item => decimal.Parse(item.Groups[1].Value, CultureInfo.InvariantCulture))
            .ToList();
        if (values.Count != dates.Count)
            throw new InvalidDataException($"IPT series {code} did not match its dates");

        return new LebanonFuelSeriesResponse(
            code,
            dates.Select((date, index) => new MarketPricePointResponse(date, values[index])).ToList());
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
}
