namespace Shabakat.Application.Helper;

public static class TriggerDateHelper
{
    public static int ResolveEffectiveDay(int preferenceTriggerDate, int year, int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        return Math.Min(preferenceTriggerDate, daysInMonth);
    }

    public static bool ShouldTriggerOn(int preferenceTriggerDate, DateOnly date)
        => ResolveEffectiveDay(preferenceTriggerDate, date.Year, date.Month) == date.Day;
}
