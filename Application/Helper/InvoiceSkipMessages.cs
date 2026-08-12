using System.Globalization;
using System.Text.RegularExpressions;

namespace Shabakat.Application.Helper;

public static class InvoiceSkipMessages
{
    public static bool IsArabic(string? language) =>
        language?.Equals("ar", StringComparison.OrdinalIgnoreCase) == true;

    public static string AllCustomersAlreadyInvoiced(string? language) =>
        IsArabic(language)
            ? "جميع العملاء النشطين لديهم فاتورة للفترة الحالية."
            : "All active customers already have an invoice for their current period.";

    public static string UnsupportedPlan(string? language, string plan) =>
        IsArabic(language)
            ? $"الخطة «{plan}» غير مدعومة لإنشاء الفاتورة."
            : $"Plan '{plan}' is not supported for invoice creation.";

    public static string DuplicatePeriod(
        string? language,
        string customerName,
        DateOnly billingPeriodStart,
        DateOnly billingPeriodEnd)
    {
        var start = billingPeriodStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var end = billingPeriodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return IsArabic(language)
            ? $"العميل «{customerName}» لديه فاتورة بالفعل للفترة من {start} إلى {end}."
            : $"{customerName} already has an invoice for the period {start} to {end}.";
    }

    public static string MissingMeterReading(
        string? language,
        string customerName,
        DateOnly consumptionStart,
        DateOnly consumptionEnd)
    {
        var start = consumptionStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var end = consumptionEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return IsArabic(language)
            ? $"لا توجد قراءة عداد مسجلة للعميل «{customerName}» للفترة من {start} إلى {end}. سجّل قراءة العداد قبل إنشاء هذه الفاتورة."
            : $"No meter reading recorded for {customerName} for the period {start} to {end}. " +
              "Record a meter reading before creating this invoice.";
    }

    public static string CustomersSkippedSummary(string? language, int skipped) =>
        skipped switch
        {
            1 when IsArabic(language) => "تم تخطي عميل واحد.",
            1 => "1 customer was skipped.",
            _ when IsArabic(language) => $"تم تخطي {skipped.ToString(CultureInfo.InvariantCulture)} عميلاً.",
            _ => $"{skipped} customers were skipped."
        };

    public static string LocalizeStoredReason(
        string? storedReason,
        string? language,
        string customerName,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        if (string.IsNullOrWhiteSpace(storedReason))
            return storedReason ?? string.Empty;

        if (IsMissingMeterReadingReason(storedReason))
            return MissingMeterReading(language, customerName, periodStart, periodEnd);

        if (IsDuplicatePeriodReason(storedReason))
            return DuplicatePeriod(language, customerName, periodStart, periodEnd);

        if (TryGetUnsupportedPlan(storedReason, out var plan))
            return UnsupportedPlan(language, plan);

        return storedReason;
    }

    private static bool IsMissingMeterReadingReason(string reason) =>
        reason.Contains("No meter reading recorded", StringComparison.OrdinalIgnoreCase) ||
        reason.Contains("لا توجد قراءة عداد", StringComparison.Ordinal);

    private static bool IsDuplicatePeriodReason(string reason) =>
        reason.Contains("already has an invoice for the period", StringComparison.OrdinalIgnoreCase) ||
        reason.Contains("لديه فاتورة بالفعل للفترة", StringComparison.Ordinal);

    private static bool TryGetUnsupportedPlan(string reason, out string plan)
    {
        plan = string.Empty;

        if (!reason.Contains("is not supported for invoice creation", StringComparison.OrdinalIgnoreCase) &&
            !reason.Contains("غير مدعومة لإنشاء الفاتورة", StringComparison.Ordinal))
            return false;

        var english = Regex.Match(reason, @"Plan '([^']+)'", RegexOptions.IgnoreCase);
        if (english.Success)
        {
            plan = english.Groups[1].Value;
            return true;
        }

        var arabic = Regex.Match(reason, @"الخطة «([^»]+)»");
        if (arabic.Success)
        {
            plan = arabic.Groups[1].Value;
            return true;
        }

        plan = "Unknown";
        return true;
    }
}
