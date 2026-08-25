namespace Shabakat.Application.Helper;

public static class InvoicePdfFilePicker
{
    public static async Task<string?> PickSavePathAsync(string fileName)
    {
        var native = NativeWindow();
        if (native is null)
            return null;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(native);
        var picker = new Windows.Storage.Pickers.FileSavePicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        picker.SuggestedFileName = Path.GetFileNameWithoutExtension(fileName);
        picker.FileTypeChoices.Add("PDF", [".pdf"]);

        var dest = await picker.PickSaveFileAsync();
        return dest?.Path;
    }

    private static Microsoft.UI.Xaml.Window? NativeWindow()
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        return window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
    }
}
