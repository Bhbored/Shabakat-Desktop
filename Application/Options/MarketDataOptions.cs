namespace Shabakat.Application.Options;

public sealed class MarketDataOptions
{
    public const string SectionName = "MarketData";

    public string YahooChartBaseUrl { get; set; } = "https://query1.finance.yahoo.com/";
    public string IptFuelPricesUrl { get; set; } = "https://www.iptgroup.com.lb/ipt/en/our-stations/fuel-prices";
    public int GlobalCacheMinutes { get; set; } = 5;
    public int LebanonCacheMinutes { get; set; } = 30;
}
