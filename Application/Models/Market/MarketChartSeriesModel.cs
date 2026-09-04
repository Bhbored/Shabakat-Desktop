using Shabakat.Application.DTOs.Market;

namespace Shabakat.Application.Models.Market;

public sealed record MarketChartSeriesModel(
    string Label,
    IReadOnlyList<MarketPricePointResponse> Points,
    string Color,
    int DashLength = 0);
