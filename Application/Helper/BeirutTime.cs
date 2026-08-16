namespace Shabakat.Application.Helper;

public static class BeirutTime
{
    public static TimeZoneInfo Zone { get; } = Resolve();

    public static DateTimeOffset ToUtc(DateOnly date, TimeOnly time)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        var offset = Zone.GetUtcOffset(local);
        return new DateTimeOffset(local, offset).ToUniversalTime();
    }

    public static DateTimeOffset ToLocal(DateTimeOffset utc) =>
        TimeZoneInfo.ConvertTime(utc, Zone);

    private static TimeZoneInfo Resolve()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Middle East Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            "Asia/Beirut",
            TimeSpan.FromHours(3),
            "Beirut",
            "Beirut");
    }
}
