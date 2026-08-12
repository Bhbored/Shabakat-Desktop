using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Services.Pricing;

public interface IPricingService
{
    PricingRates GetRates(Customer customer, AppPreferences preferences);

    PricingRates GetRates(
        CustomerType customerType,
        PlanType plan,
        AppPreferences preferences);
}
