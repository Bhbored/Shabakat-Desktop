using System.Globalization;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.Helper;

namespace Shabakat.Application.Services.Culture;

public sealed class CultureService : ICultureService
{
    public string Language { get; private set; } = "en";

    public bool IsRtl => string.Equals(Language, "ar", StringComparison.OrdinalIgnoreCase);

    public CultureInfo ResourceCulture { get; private set; } = CultureInfo.GetCultureInfo("en");

    public CultureInfo FormatCulture { get; private set; } = CultureInfo.GetCultureInfo("en-US");

    public event Action? Changed;

    public void Apply(string language)
    {
        var isAr = language.Trim().StartsWith("ar", StringComparison.OrdinalIgnoreCase);
        Language = isAr ? "ar" : "en";
        ResourceCulture = CultureInfo.GetCultureInfo(isAr ? "ar" : "en");
        FormatCulture = CultureInfo.GetCultureInfo(isAr ? "ar-LB" : "en-US");
        FormatHelper.Culture = FormatCulture;
        Changed?.Invoke();
    }
}
