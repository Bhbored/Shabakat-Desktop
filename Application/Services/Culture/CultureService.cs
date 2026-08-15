using System.Globalization;
using Shabakat.Application.Contracts.Services;

namespace Shabakat.Application.Services.Culture;

public sealed class CultureService : ICultureService
{
    public string Language { get; private set; } = "en";

    public bool IsRtl => string.Equals(Language, "ar", StringComparison.OrdinalIgnoreCase);

    public event Action? Changed;

    public void Apply(string language)
    {
        var isAr = language.Trim().StartsWith("ar", StringComparison.OrdinalIgnoreCase);
        Language = isAr ? "ar" : "en";
        var culture = CultureInfo.GetCultureInfo(isAr ? "ar-LB" : "en-US");

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        Changed?.Invoke();
    }
}
