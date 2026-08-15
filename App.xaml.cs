using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Shabakat.Application.Contracts.Services;
using WinUIFlowDirection = Microsoft.UI.Xaml.FlowDirection;
using WinUIFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WinUIWindow = Microsoft.UI.Xaml.Window;

namespace Shabakat
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage()) { Title = "Shabakat" };
            PinNativeChrome(window);
            window.HandlerChanged += (_, _) => PinNativeChrome(window);

            var culture = IPlatformApplication.Current?.Services.GetService<ICultureService>();
            if (culture is not null)
                culture.Changed += () => MainThread.BeginInvokeOnMainThread(() => PinNativeChrome(window));

            return window;
        }

        private static void PinNativeChrome(Window window)
        {
            try
            {
                global::Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
            }
            catch
            {
            }

            if (window.Handler?.PlatformView is WinUIWindow native
                && native.Content is WinUIFrameworkElement content)
            {
                content.FlowDirection = WinUIFlowDirection.LeftToRight;
            }
        }
    }
}
