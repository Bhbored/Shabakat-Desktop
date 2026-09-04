using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Market;

public sealed record MarketPricePointResponse(DateTimeOffset Timestamp, decimal Value);

public sealed record CommodityQuoteResponse(
    string Code,
    string Symbol,
    decimal Price,
    decimal Change,
    decimal ChangePercent,
    string Currency,
    string Unit,
    DateTimeOffset MarketTime,
    IReadOnlyList<MarketPricePointResponse> History);

public sealed record GlobalMarketResponse(
    MarketRange Range,
    IReadOnlyList<CommodityQuoteResponse> Quotes,
    DateTimeOffset FetchedAt);

public sealed record LebanonFuelQuoteResponse(string Code, decimal Price, string Currency);

public sealed record LebanonFuelSeriesResponse(
    string Code,
    IReadOnlyList<MarketPricePointResponse> History);

public sealed record LebanonMarketResponse(
    IReadOnlyList<LebanonFuelQuoteResponse> Quotes,
    IReadOnlyList<LebanonFuelSeriesResponse> Series,
    DateTimeOffset SourceDate,
    DateTimeOffset FetchedAt);
