using Shabakat.Application.DTOs.Market;

namespace Shabakat.Components.Features.Market.Components;

public sealed record MarketChartSeriesModel(
    string Label,
    IReadOnlyList<MarketPricePointResponse> Points,
    string Color,
    int DashLength = 0);
