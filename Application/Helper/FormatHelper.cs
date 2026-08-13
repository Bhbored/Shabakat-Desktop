using System.Globalization;

namespace Shabakat.Application.Helper;

public static class FormatHelper
{
    private static readonly CultureInfo EnUs = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo EnGb = CultureInfo.GetCultureInfo("en-GB");

    public static string Currency(decimal value) =>
        value.ToString("C2", EnUs);

    public static string CompactCurrency(decimal value) =>
        value.ToString("C0", EnUs);

    public static string Number(decimal value) =>
        value.ToString("N2", EnUs);

    public static string Number(int value) =>
        value.ToString("N0", EnUs);

    public static string Date(DateOnly value) =>
        value.ToString("dd MMM yyyy", EnGb);

    public static string Date(DateTime value) =>
        value.ToString("dd MMM yyyy", EnGb);

    public static string Initials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
    }

    private static readonly string[] AvatarColors =
    [
        "#7c3aed", "#2563eb", "#059669", "#db2777", "#ea580c", "#0891b2", "#d97706"
    ];

    public static string AvatarColor(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return AvatarColors[0];

        return AvatarColors[Math.Abs(name.GetHashCode()) % AvatarColors.Length];
    }
}
