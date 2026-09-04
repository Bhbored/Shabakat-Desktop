using Shabakat.Application.DTOs.Market;

namespace Shabakat.Application.Models.Market;

public sealed class MarketCacheFile
{
    public Dictionary<string, GlobalMarketResponse> Global { get; set; } = [];
    public LebanonMarketResponse? Lebanon { get; set; }
}
