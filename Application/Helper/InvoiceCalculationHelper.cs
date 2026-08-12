using Shabakat.Domain.Entities;

namespace Shabakat.Application.Helper;

public static class InvoiceCalculationHelper
{
    public static decimal CalculateAmpereTotal(
        decimal planValue,
        decimal unitPrice,
        decimal fixedCharge,
        decimal tvaPercent,
        int? billedDays = null,
        int? daysInMonth = null)
    {
        var subscription = unitPrice * planValue;
        var baseAmount = subscription + fixedCharge;

        if (billedDays.HasValue && daysInMonth is > 0)
        {
            var factor = billedDays.Value / (decimal)daysInMonth.Value;
            baseAmount *= factor;
        }

        return Math.Round(baseAmount * (1 + tvaPercent / 100m), 4);
    }

    public static decimal CalculateKilowattTotal(
        decimal consumption,
        decimal planValue,
        decimal unitPrice,
        decimal fixedCharge,
        decimal tvaPercent)
    {
        var baseAmount = consumption * unitPrice + fixedCharge;
        return Math.Round(baseAmount * (1 + tvaPercent / 100m), 4) + planValue;
    }

    public static decimal CalculateFixedKilowattConsumption(
        decimal paymentAmount,
        decimal planValue,
        decimal unitPrice,
        decimal fixedCharge,
        decimal tvaPercent)
    {
        var energyTotal = paymentAmount - planValue;
        if (energyTotal <= 0)
            return 0m;

        var baseAmount = energyTotal / (1 + tvaPercent / 100m);
        var energyBeforeFixed = baseAmount - fixedCharge;
        if (energyBeforeFixed <= 0)
            return 0m;

        return Math.Round(energyBeforeFixed / unitPrice, 4);
    }

    public static decimal CalculateConsumption(
        decimal currentReading,
        MeterReading? previousReading)
        => previousReading is null
            ? currentReading
            : currentReading - previousReading.ReadingValue;
}
