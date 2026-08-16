using Microsoft.UI.Xaml;

namespace Shabakat.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            var baseDir = AppContext.BaseDirectory;
            if (!string.IsNullOrWhiteSpace(baseDir))
                Directory.SetCurrentDirectory(baseDir);

            var webViewData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Shabakat",
                "WebView2");
            Directory.CreateDirectory(webViewData);
            Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", webViewData);

            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
