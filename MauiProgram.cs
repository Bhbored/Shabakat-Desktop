using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.DevFlow.Agent;
using Shabakat.Application.Contracts.Services;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try
            {
                return CreateMauiAppCore();
            }
            catch (Exception ex)
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Shabakat");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "startup-error.log"), ex.ToString());
                throw;
            }
        }

        private static MauiApp CreateMauiAppCore()
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

            builder.Services.AddMauiBlazorWebView();
            builder.Services.RegisterDependencies(builder.Configuration);

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.SetMinimumLevel(LogLevel.Debug);
    		builder.Logging.AddDebug();
            builder.AddMauiDevFlowAgent();
#endif

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();

                var prefs = scope.ServiceProvider.GetRequiredService<IAppPreferencesService>()
                    .GetAsync()
                    .GetAwaiter()
                    .GetResult();
                scope.ServiceProvider.GetRequiredService<ICultureService>()
                    .Apply(prefs?.Language ?? "en");
            }

            return app;
        }
    }
}
