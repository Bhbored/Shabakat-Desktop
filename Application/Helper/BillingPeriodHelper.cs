using Shabakat.Domain.Enums;

namespace Shabakat.Application.Helper;

public static class BillingPeriodHelper
{
    public static bool UsesKilowattPreviousMonthBilling(DateOnly referenceToday)
        => referenceToday.Day is 1 or 2;

    public static DateOnly GetBillingMonthStart(PlanType plan, DateOnly referenceToday)
    {
        var currentMonthStart = new DateOnly(referenceToday.Year, referenceToday.Month, 1);

        if (plan == PlanType.Kilowatt && UsesKilowattPreviousMonthBilling(referenceToday))
            return currentMonthStart.AddMonths(-1);

        return currentMonthStart;
    }

    public static (DateOnly MonthStart, DateOnly MonthEnd) GetBillingMonthBounds(
        PlanType plan,
        DateOnly referenceToday)
    {
        var monthStart = GetBillingMonthStart(plan, referenceToday);
        return (monthStart, monthStart.AddMonths(1).AddDays(-1));
    }

    public static (DateOnly ConsumptionStart, DateOnly ConsumptionEnd) ResolveConsumptionPeriod(
        PlanType plan,
        DateOnly referenceToday)
    {
        var (monthStart, monthEnd) = GetBillingMonthBounds(plan, referenceToday);

        var consumptionStart = plan == PlanType.Ampere
            ? referenceToday
            : monthStart;

        return (consumptionStart, monthEnd);
    }

    public static DateOnly ResolvePaymentDueDate(DateOnly referenceToday, int preferenceDueDate)
    {
        var thisMonthDay = Math.Min(
            preferenceDueDate,
            DateTime.DaysInMonth(referenceToday.Year, referenceToday.Month));

        if (referenceToday.Day <= thisMonthDay)
            return new DateOnly(referenceToday.Year, referenceToday.Month, thisMonthDay);

        var nextMonth = referenceToday.AddMonths(1);
        var nextMonthDay = Math.Min(
            preferenceDueDate,
            DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
        return new DateOnly(nextMonth.Year, nextMonth.Month, nextMonthDay);
    }

    public static DateOnly ResolveKilowattMeterReadingPeriodEnd(
        DateOnly consumptionEnd,
        DateOnly referenceToday)
    {
        var consumptionEndExclusive = consumptionEnd.AddDays(1);
        var todayEnd = referenceToday.AddDays(1);
        return consumptionEndExclusive > todayEnd ? consumptionEndExclusive : todayEnd;
    }
}
