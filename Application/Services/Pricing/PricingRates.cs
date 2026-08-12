namespace Shabakat.Application.Services.Pricing;

public sealed record PricingRates(
    decimal UnitPrice,
    decimal FixedCharge,
    decimal Tva);
