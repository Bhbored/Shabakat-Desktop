using System.Globalization;

namespace Shabakat.Application.Helper;

public static class FormatHelper
{
    private static readonly CultureInfo UsdCulture = CultureInfo.GetCultureInfo("en-US");

    public static CultureInfo Culture { get; set; } = CultureInfo.GetCultureInfo("en-US");

    public static string Currency(decimal value) =>
        value.ToString("C2", UsdCulture);

    public static string CompactCurrency(decimal value) =>
        value.ToString("C0", UsdCulture);

    public static string Number(decimal value) =>
        value.ToString("N2", Culture);

    public static string Number(int value) =>
        value.ToString("N0", Culture);

    public static string Date(DateOnly value) =>
        value.ToString("dd MMM yyyy", Culture);

    public static string Date(DateTime value) =>
        value.ToString("dd MMM yyyy", Culture);

    /// <summary>Invoice print header: <c>24-Aug-26</c> (en) or <c>24-آب-26</c> (ar, Levantine months, western digits).</summary>
    public static string InvoicePrintDate(DateOnly value, bool arabic) =>
        arabic ? ArabicPrintDate(value.Day, value.Month, value.Year) : value.ToString("dd-MMM-yy", UsdCulture);

    public static string InvoicePrintDate(DateTime value, bool arabic) =>
        InvoicePrintDate(DateOnly.FromDateTime(value), arabic);

    public static string MonthYear(int year, int month)
    {
        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month));

        if (IsArabic)
            return $"{LevantineMonths[month - 1]} {year.ToString(CultureInfo.InvariantCulture)}";

        return new DateOnly(year, month, 1).ToString("MMMM yyyy", UsdCulture);
    }

    private static bool IsArabic =>
        Culture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase);

    private static readonly string[] LevantineMonths =
    [
        "كانون الثاني",
        "شباط",
        "آذار",
        "نيسان",
        "أيار",
        "حزيران",
        "تموز",
        "آب",
        "أيلول",
        "تشرين الأول",
        "تشرين الثاني",
        "كانون الأول"
    ];

    private static string ArabicPrintDate(int day, int month, int year) =>
        $"{day.ToString("00", CultureInfo.InvariantCulture)}-{LevantineMonths[month - 1]}-{(year % 100).ToString("00", CultureInfo.InvariantCulture)}";

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
