namespace Shabakat.Infrastructure.Persistence.Seed;

internal static class SeedIds
{
    public const byte Area = 1;
    public const byte Schedule = 2;
    public const byte Box = 3;
    public const byte Customer = 4;
    public const byte Reading = 5;
    public const byte Invoice = 6;
    public const byte Payment = 7;
    public const byte Expense = 8;
    public const byte Skip = 9;

    public static Guid For(byte kind, int index)
        => Guid.Parse($"a{kind:x1}000001-0000-4000-8000-{index:x12}");
}
