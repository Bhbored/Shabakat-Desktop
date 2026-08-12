namespace Shabakat.Application.Contracts.Services;

public enum AppThemeMode
{
    Light,
    Dark,
    System
}

public interface IThemeService
{
    AppThemeMode Mode { get; }
    string ResolvedTheme { get; }
    event Action? Changed;
    void SetTheme(AppThemeMode mode);
}
