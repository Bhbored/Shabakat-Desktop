using System.Globalization;
using System.Text;
using Shabakat.Application.Contracts.Abstractions;
using Shabakat.Application.DTOs.Invoices;

namespace Shabakat.Application.Services.Invoices;

public sealed class InvoiceTemplateRenderer : IInvoiceTemplateRenderer
{
    private static readonly Lazy<string> TemplateEn = new(() => LoadTemplate("invoice.html"));
    private static readonly Lazy<string> TemplateAr = new(() => LoadTemplate("invoice.ar.html"));
    private static readonly Lazy<string> ArabicFontStyle = new(BuildArabicFontStyle);

    public string Render(InvoicePrintModel model, string language = "en")
    {
        var isArabic = IsArabic(language);
        var html = isArabic ? TemplateAr.Value : TemplateEn.Value;
        var tokens = BuildTokens(model, isArabic);

        foreach (var (key, value) in tokens)
            html = html.Replace(key, value, StringComparison.Ordinal);

        if (isArabic)
            html = InjectArabicFonts(html);

        return html;
    }

    private static bool IsArabic(string language)
        => language.Equals("ar", StringComparison.OrdinalIgnoreCase);

    private static Dictionary<string, string> BuildTokens(InvoicePrintModel m, bool isArabic)
    {
        var logoHtml = BuildLogoHtml(m.CompanyName, m.LogoUrl);

        var phoneLabel = isArabic ? "الهاتف" : "Phone";
        var addressLabel = isArabic ? "العنوان" : "Address";

        var customerPhone = string.IsNullOrWhiteSpace(m.CustomerPhone)
            ? string.Empty
            : $"""<div class="field"><label>{phoneLabel}</label><span>{Escape(m.CustomerPhone)}</span></div>""";

        var customerAddress = string.IsNullOrWhiteSpace(m.CustomerAddress)
            ? string.Empty
            : $"""<div class="field"><label>{addressLabel}</label><span>{Escape(m.CustomerAddress)}</span></div>""";

        var planType = LocalizePlanType(m.PlanType, isArabic);
        var planUnit = ResolvePlanUnit(m, isArabic);

        var showMeterReadings = m.IsKilowattPlan || m.IsFixedKilowattPlan;

        var readingsSection = showMeterReadings
            ? BuildReadingsSection(m, isArabic)
            : string.Empty;

        var consumptionRow = BuildConsumptionRow(m, isArabic);
        var tvaRows = BuildTvaRows(m, isArabic);
        var isAmperePlan = m is { IsKilowattPlan: false, IsFixedKilowattPlan: false };
        var stubUnitPriceHtml = isAmperePlan
            ? BuildStubField(isArabic ? "سعر الوحدة" : "Unit Price", Money(m.UnitPrice))
            : string.Empty;
        var cableName = string.IsNullOrWhiteSpace(m.CableName) ? "—" : m.CableName;

        return new Dictionary<string, string>
        {
            ["{{COMPANY_NAME}}"] = Escape(m.CompanyName),
            ["{{LOGO_HTML}}"] = logoHtml,
            ["{{INVOICE_NUMBER}}"] = m.InvoiceNumber.ToString(CultureInfo.InvariantCulture),
            ["{{CONSUMPTION_START}}"] = Escape(m.ConsumptionStart),
            ["{{CONSUMPTION_END}}"] = Escape(m.ConsumptionEnd),
            ["{{DUE_DATE}}"] = Escape(m.DueDate),
            ["{{CREATED_DATE}}"] = Escape(m.CreatedDate),
            ["{{CUSTOMER_NAME}}"] = Escape(m.CustomerName),
            ["{{CUSTOMER_PHONE_HTML}}"] = customerPhone,
            ["{{CUSTOMER_ADDRESS_HTML}}"] = customerAddress,
            ["{{CABLE_NAME}}"] = Escape(cableName),
            ["{{PLAN_TYPE}}"] = Escape(planType),
            ["{{PLAN_VALUE}}"] = FormatDecimal(m.PlanValue),
            ["{{PLAN_UNIT}}"] = planUnit,
            ["{{UNIT_PRICE}}"] = Money(m.UnitPrice),
            ["{{STUB_UNIT_PRICE_HTML}}"] = stubUnitPriceHtml,
            ["{{READINGS_SECTION}}"] = readingsSection,
            ["{{CONSUMPTION_ROW}}"] = consumptionRow,
            ["{{FIXED_CHARGE}}"] = Money(m.FixedCharge),
            ["{{TVA_ROWS}}"] = tvaRows,
            ["{{TOTAL_AMOUNT}}"] = Money(m.TotalAmount),
            ["{{PAID_AMOUNT}}"] = Money(m.PaidAmount),
            ["{{AMOUNT_DUE}}"] = Money(m.AmountDue),
        };
    }

    private static string LocalizePlanType(string planType, bool isArabic)
    {
        if (!isArabic)
            return planType;

        return planType switch
        {
            "Ampere" => "أمبير",
            "Kilowatt" => "كيلوواط",
            "FixedKilowatt" => "كيلوواط ثابت",
            _ => planType
        };
    }

    private static string ResolvePlanUnit(InvoicePrintModel m, bool isArabic)
    {
        if (isArabic)
        {
            if (m.IsFixedKilowattPlan) return "كيلوواط مسبق الدفع";
            if (m.IsKilowattPlan) return "حد كيلوواط";
            return "أمبير";
        }

        if (m.IsFixedKilowattPlan) return "kWh prepaid";
        if (m.IsKilowattPlan) return "kWh limit";
        return "Amperes";
    }

    private static string BuildStubField(string label, string value)
        => $"""<div class="stub-field"><label>{Escape(label)}</label><span>{value}</span></div>""";

    private static string BuildReadingsSection(InvoicePrintModel m, bool isArabic)
    {
        var title = isArabic ? "قراءات العداد" : "Meter Readings";
        var previousLabel = isArabic ? "القراءة السابقة" : "Previous Reading";
        var currentLabel = isArabic ? "القراءة الحالية" : "Current Reading";
        var totalLabel = isArabic ? "إجمالي الاستهلاك" : "Total Consumption";
        var kwhUnit = isArabic ? "كيلوواط" : "kWh";

        return $"""
                <div class="readings-block">
                <h2 class="section-title">{title}</h2>
                <div class="readings">
                  <div class="reading-card">
                    <label>{previousLabel}</label>
                    <div class="value">{FormatReading(m.PreviousReading)}</div>
                    {(m.PreviousReadingDate is not null ? $"""<div class="date">{Escape(m.PreviousReadingDate)}</div>""" : "")}
                  </div>
                  <div class="reading-card">
                    <label>{currentLabel}</label>
                    <div class="value">{FormatReading(m.CurrentReading)}</div>
                    {(m.CurrentReadingDate is not null ? $"""<div class="date">{Escape(m.CurrentReadingDate)}</div>""" : "")}
                  </div>
                  <div class="reading-card highlight">
                    <label>{totalLabel}</label>
                    <div class="value">{FormatConsumption(m.TotalConsumption, kwhUnit)}</div>
                  </div>
                </div>
                </div>
                """;
    }

    private static string BuildConsumptionRow(InvoicePrintModel m, bool isArabic)
    {
        if (m.IsFixedKilowattPlan)
        {
            var perUnit = isArabic ? "/كيلوواط" : "/kWh";
            var label = isArabic ? "رصيد الطاقة" : "Energy credit";
            var unit = isArabic ? "كيلوواط" : "kWh";
            return $"""
                    <tr>
                      <td>{label} ({FormatDecimal(m.TotalConsumption ?? 0m)} {unit} × {Money(m.UnitPrice)}{perUnit})</td>
                      <td>{Money(m.ConsumptionCost)}</td>
                    </tr>
                    """;
        }

        if (m.IsKilowattPlan)
        {
            var label = isArabic ? "استهلاك الطاقة" : "Energy consumption";
            var unit = isArabic ? "كيلوواط" : "kWh";
            return $"""
                    <tr>
                      <td>{label} ({FormatDecimal(m.TotalConsumption ?? 0m)} {unit} × {Money(m.UnitPrice)})</td>
                      <td>{Money(m.ConsumptionCost)}</td>
                    </tr>
                    """;
        }

        var ampereUnit = isArabic ? "أمبير" : "Amperes";
        var subscriptionLabel = isArabic ? "الاشتراك" : "Subscription";
        return $"""
                <tr>
                  <td>{subscriptionLabel} ({FormatDecimal(m.PlanValue)} {ampereUnit} × {Money(m.UnitPrice)})</td>
                  <td>{Money(m.ConsumptionCost)}</td>
                </tr>
                """;
    }

    private static string BuildTvaRows(InvoicePrintModel m, bool isArabic)
    {
        if (!m.ShowTva)
            return string.Empty;

        var subtotalLabel = isArabic ? "المجموع قبل الضريبة" : "Subtotal before TVA";
        var tvaLabel = isArabic ? "ضريبة القيمة المضافة" : "TVA";

        return $"""
                <tr>
                  <td>{subtotalLabel}</td>
                  <td>{Money(m.SubtotalBeforeTva)}</td>
                </tr>
                <tr>
                  <td>{tvaLabel} ({FormatDecimal(m.TvaPercent)}%)</td>
                  <td>{Money(m.TvaAmount)}</td>
                </tr>
                """;
    }

    private static string BuildLogoHtml(string companyName, string? logoUrl)
    {
        if (TryGetLogoUrl(logoUrl, out var resolvedUrl))
        {
            return $"""
                    <img class="logo" src="{Escape(resolvedUrl)}" alt="{Escape(companyName)} logo" />
                    """;
        }

        var initial = companyName.Length > 0 ? companyName[0].ToString() : "?";
        return $"""<div class="logo-placeholder">{Escape(initial)}</div>""";
    }

    private static bool TryGetLogoUrl(string? logoUrl, out string resolvedUrl)
    {
        resolvedUrl = string.Empty;

        if (string.IsNullOrWhiteSpace(logoUrl))
            return false;

        var trimmed = logoUrl.Trim();

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
        {
            resolvedUrl = trimmed;
            return true;
        }

        if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            resolvedUrl = trimmed;
            return true;
        }

        var path = trimmed.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
            ? new Uri(trimmed).LocalPath
            : trimmed;

        if (!File.Exists(path))
            return false;

        var mime = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };

        var bytes = File.ReadAllBytes(path);
        resolvedUrl = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        return true;
    }

    private static string InjectArabicFonts(string html)
    {
        var style = ArabicFontStyle.Value;
        if (string.IsNullOrEmpty(style))
            return html;

        const string headClose = "</head>";
        var index = html.IndexOf(headClose, StringComparison.OrdinalIgnoreCase);
        return index >= 0
            ? html.Insert(index, style)
            : style + html;
    }

    private static string BuildArabicFontStyle()
    {
        var fontsDir = Path.Combine(AppContext.BaseDirectory, "Templates", "Fonts");
        var regularPath = Path.Combine(fontsDir, "NotoSansArabic-Regular.ttf");
        var boldPath = Path.Combine(fontsDir, "NotoSansArabic-Bold.ttf");

        if (!File.Exists(regularPath) || !File.Exists(boldPath))
            return string.Empty;

        var regular = Convert.ToBase64String(File.ReadAllBytes(regularPath));
        var bold = Convert.ToBase64String(File.ReadAllBytes(boldPath));

        var sb = new StringBuilder();
        sb.Append("<style>");
        sb.Append("@font-face{font-family:'Noto Sans Arabic';src:url('data:font/ttf;base64,");
        sb.Append(regular);
        sb.Append("') format('truetype');font-weight:400;font-style:normal;}");
        sb.Append("@font-face{font-family:'Noto Sans Arabic';src:url('data:font/ttf;base64,");
        sb.Append(bold);
        sb.Append("') format('truetype');font-weight:700;font-style:normal;}");
        sb.Append("body,.invoice{font-family:'Noto Sans Arabic',Tahoma,sans-serif;}");
        sb.Append("</style>");
        return sb.ToString();
    }

    private static string LoadTemplate(string fileName)
    {
        try
        {
            using var package = FileSystem.OpenAppPackageFileAsync($"Templates/{fileName}").GetAwaiter().GetResult();
            using var reader = new StreamReader(package);
            return reader.ReadToEnd();
        }
        catch (Exception)
        {
            // ignored
        }

        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Templates", fileName),
            Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Templates", fileName)
        };

        var path = candidates.FirstOrDefault(File.Exists)
                   ?? throw new FileNotFoundException($"Invoice template not found: {fileName}");

        return File.ReadAllText(path);
    }

    private static string Escape(string? value)
        => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);

    private static string Money(decimal value)
        => $"${FormatAmount(value)}";

    private static string FormatDecimal(decimal value)
        => FormatAmount(value);

    private static string FormatAmount(decimal value)
    {
        var rounded = Math.Round(value, 2, MidpointRounding.AwayFromZero);
        if (rounded == decimal.Truncate(rounded))
            return rounded.ToString("0", CultureInfo.InvariantCulture);

        return rounded.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatReading(decimal? value)
        => value is null ? "—" : FormatDecimal(value.Value);

    private static string FormatConsumption(decimal? value, string kwhUnit)
        => value is null ? "—" : $"{FormatDecimal(value.Value)} {kwhUnit}";
}
