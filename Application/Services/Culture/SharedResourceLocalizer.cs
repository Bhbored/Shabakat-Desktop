using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Localization;
using Shabakat.Application.Contracts.Services;
using Shabakat.Resources.Localization;

namespace Shabakat.Application.Services.Culture;

public sealed class SharedResourceLocalizer : IStringLocalizer<SharedResource>
{
    private static readonly ResourceManager Manager = new(
        typeof(SharedResource).FullName!,
        typeof(SharedResource).Assembly);

    private readonly ICultureService _culture;

    public SharedResourceLocalizer(ICultureService culture)
    {
        _culture = culture;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = Manager.GetString(name, _culture.ResourceCulture);
            return new LocalizedString(name, value ?? name, resourceNotFound: value is null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var format = Manager.GetString(name, _culture.ResourceCulture);
            var value = format is null
                ? name
                : string.Format(_culture.FormatCulture, format, arguments);
            return new LocalizedString(name, value, resourceNotFound: format is null);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}
