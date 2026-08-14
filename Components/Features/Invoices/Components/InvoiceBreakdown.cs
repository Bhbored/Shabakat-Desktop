using Shabakat.Domain.Enums;

namespace Shabakat.Components.Features.Invoices.Components;

internal static class InvoiceBreakdown
{
    public static (decimal Charge, decimal FixedCharge, decimal TvaAmount, decimal TvaRate) Compute(
        decimal totalAmount,
        decimal fixedCharge,
        decimal tva,
        decimal planValue,
        bool includePlanValue)
    {
        var extra = includePlanValue ? planValue : 0m;
        var taxableTotal = totalAmount - extra;

        decimal charge;
        decimal tvaAmount;
        if (tva <= 0)
        {
            charge = taxableTotal - fixedCharge;
            tvaAmount = 0;
        }
        else
        {
            var rate = tva / 100m;
            var subtotal = taxableTotal / (1 + rate);
            charge = subtotal - fixedCharge;
            tvaAmount = subtotal * rate;
        }

        return (charge + extra, fixedCharge, tvaAmount, tva);
    }

    public static bool IncludePlanValue(string? plan) =>
        plan?.Equals(nameof(PlanType.Kilowatt), StringComparison.OrdinalIgnoreCase) == true;
}
