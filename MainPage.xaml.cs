using Microsoft.AspNetCore.Components.WebView;

namespace Shabakat
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnWebViewInitializing(object? sender, BlazorWebViewInitializingEventArgs e)
        {
            var folder = Path.Combine(FileSystem.AppDataDirectory, "WebView2");
            Directory.CreateDirectory(folder);
            e.UserDataFolder = folder;
        }
    }
}
