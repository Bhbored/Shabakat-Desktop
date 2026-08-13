namespace Shabakat.Application.Services.Theme;

using Shabakat.Application.Contracts.Services;

public sealed class ThemeService : IThemeService
{
    private const string PreferenceKey = "shabakat-theme";

    public AppThemeMode Mode { get; private set; }

    public string ResolvedTheme => Resolve(Mode);

    public event Action? Changed;

    public ThemeService()
    {
        var saved = Microsoft.Maui.Storage.Preferences.Default.Get(PreferenceKey, "dark");
        Mode = saved switch
        {
            "light" => AppThemeMode.Light,
            "system" => AppThemeMode.System,
            _ => AppThemeMode.Dark
        };
    }

    public void SetTheme(AppThemeMode mode)
    {
        Mode = mode;
        Microsoft.Maui.Storage.Preferences.Default.Set(PreferenceKey, mode switch
        {
            AppThemeMode.Light => "light",
            AppThemeMode.System => "system",
            _ => "dark"
        });
        Changed?.Invoke();
    }

    private static string Resolve(AppThemeMode mode)
    {
        if (mode == AppThemeMode.Light)
            return "light";
        if (mode == AppThemeMode.Dark)
            return "dark";

        var appTheme = Microsoft.Maui.Controls.Application.Current?.RequestedTheme
            ?? Microsoft.Maui.ApplicationModel.AppTheme.Unspecified;
        return appTheme == Microsoft.Maui.ApplicationModel.AppTheme.Light ? "light" : "dark";
    }
}
