using System.Globalization;

namespace Shabakat.Application.Contracts.Services;

public interface ICultureService
{
    string Language { get; }
    bool IsRtl { get; }
    CultureInfo ResourceCulture { get; }
    CultureInfo FormatCulture { get; }
    event Action? Changed;
    void Apply(string language);
}
