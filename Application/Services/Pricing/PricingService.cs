using Microsoft.Extensions.Logging;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Pricing;

public sealed class PricingService : IPricingService
{
    private readonly ILogger<PricingService> _logger;

    public PricingService(ILogger<PricingService> logger)
    {
        _logger = logger;
    }

    public PricingRates GetRates(Customer customer, AppPreferences preferences)
    {
        if (customer.HasPricingOverride && customer.PriceOverride is > 0)
        {
            _logger.LogDebug(
                "Using pricing override for customer {CustomerId}",
                customer.Id);

            return new PricingRates(
                customer.PriceOverride!.Value,
                customer.FixedChargeOverride ?? 0m,
                customer.TVAOverride ?? 0m);
        }

        var (baseUnitPrice, fixedCharge, tva) = ResolveBaseUnitAndCharges(
            customer.CustomerType,
            customer.Plan,
            preferences);

        var unitPrice = ResolveUnitPrice(customer, preferences, baseUnitPrice);

        return new PricingRates(unitPrice, fixedCharge, tva);
    }

    public PricingRates GetRates(
        CustomerType customerType,
        PlanType plan,
        AppPreferences preferences)
    {
        var (unitPrice, fixedCharge, tva) = ResolveBaseUnitAndCharges(
            customerType,
            plan,
            preferences);

        return new PricingRates(unitPrice, fixedCharge, tva);
    }

    private static decimal ResolveUnitPrice(
        Customer customer,
        AppPreferences preferences,
        decimal baseUnitPrice)
    {
        if (customer.Plan != PlanType.Ampere || !preferences.AmpereSchedulePricingEnabled)
            return baseUnitPrice;

        if (customer.AmpereScheduleId is null)
        {
            throw new DomainException("Error.AmpereScheduleRequiredForCustomer");
        }

        if (customer.AmpereSchedule is null)
        {
            throw new DomainException("Error.AmpereScheduleDetailsMissing");
        }

        var schedule = customer.AmpereSchedule;
        var typePrice = customer.CustomerType switch
        {
            CustomerType.Residential => schedule.ResidentialPricePerAmp,
            CustomerType.Commercial => schedule.CommercialPricePerAmp,
            CustomerType.Industrial => schedule.IndustrialPricePerAmp,
            _ => 0m
        };

        var unitPrice = typePrice > 0 ? typePrice : schedule.PricePerAmp;

        if (unitPrice <= 0)
        {
            throw DomainException.Format(
                "Error.SchedulePriceInvalid",
                schedule.Name,
                $"CustomerType.{customer.CustomerType}");
        }

        return unitPrice;
    }

    private static (decimal UnitPrice, decimal FixedCharge, decimal Tva) ResolveBaseUnitAndCharges(
        CustomerType customerType,
        PlanType plan,
        AppPreferences preferences)
    {
        var (typeAmp, typeKilowat, typeFixedCharge, typeTva) = GetTypeRates(customerType, preferences);
        var typeRate = plan == PlanType.Ampere ? typeAmp : typeKilowat;

        if (typeRate > 0)
            return (typeRate, typeFixedCharge, typeTva);

        return plan == PlanType.Ampere
            ? (preferences.PricePerAmp, preferences.FixedCharge, preferences.TVA)
            : (preferences.PricePerKilowat, preferences.FixedCharge, preferences.TVA);
    }

    private static (decimal Amp, decimal Kilowat, decimal FixedCharge, decimal Tva) GetTypeRates(
        CustomerType customerType,
        AppPreferences preferences) =>
        customerType switch
        {
            CustomerType.Residential => (
                preferences.ResidentialPricePerAmp,
                preferences.ResidentialPricePerKilowat,
                preferences.ResidentialFixedCharge,
                preferences.ResidentialTVA),
            CustomerType.Commercial => (
                preferences.CommercialPricePerAmp,
                preferences.CommercialPricePerKilowat,
                preferences.CommercialFixedCharge,
                preferences.CommercialTVA),
            CustomerType.Industrial => (
                preferences.IndustrialPricePerAmp,
                preferences.IndustrialPricePerKilowat,
                preferences.IndustrialFixedCharge,
                preferences.IndustrialTVA),
            _ => (0, 0, 0, 0)
        };
}
