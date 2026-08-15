namespace Shabakat.Application.Contracts.Services;

public interface ICultureService
{
    string Language { get; }
    bool IsRtl { get; }
    event Action? Changed;
    void Apply(string language);
}
