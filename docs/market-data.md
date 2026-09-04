# Market data

Shabakat's Fuel Market page and dashboard preview are optional online features. Core subscriber, billing, payment, expense, and reporting workflows continue to use the local SQLite database when no internet connection is available.

## Sources and instruments

| Source | Data |
|---|---|
| IPT Group Lebanon | 95- and 98-octane gasoline, diesel, LPG, and published price history |
| Yahoo Finance | WTI, Brent, natural gas, RBOB gasoline, heating oil, and 1M/3M/1Y history |

No API key is required. Prices are informational and may be delayed, unavailable, or changed by their upstream source. Source links are displayed in the UI.

## Cache and connectivity

Successful responses are stored in `FileSystem.AppDataDirectory/market-prices-cache-v1.json`, not SQLite or MAUI Preferences. The default freshness windows configured in `appsettings.json` are five minutes for global data and 30 minutes for Lebanon data.

When a request fails or the device is offline, the UI displays cached data with a stale-data notice when available. Otherwise, it shows a localized section-specific error with a retry action and reports the failure through the shared toast service. Raw host names, endpoints, and exception details are not shown to users.

## Configuration

The `MarketData` section in `appsettings.json` controls source URLs and cache freshness:

```json
"MarketData": {
  "YahooChartBaseUrl": "https://query1.finance.yahoo.com/",
  "IptFuelPricesUrl": "https://www.iptgroup.com.lb/ipt/en/our-stations/fuel-prices",
  "GlobalCacheMinutes": 5,
  "LebanonCacheMinutes": 30
}
```
