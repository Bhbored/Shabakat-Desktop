namespace Shabakat.Application.Helper;

public static class BackupFilePicker
{
    public static async Task<string?> PickSavePathAsync(string suggestedFileName)
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        var native = window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (native is null)
            return null;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(native);
        var picker = new Windows.Storage.Pickers.FileSavePicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
        picker.SuggestedFileName = Path.GetFileNameWithoutExtension(suggestedFileName);
        picker.FileTypeChoices.Add("JSON backup", [".json"]);

        var dest = await picker.PickSaveFileAsync();
        return dest?.Path;
    }

    public static async Task<string?> OpenAsync()
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        var native = window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (native is null)
            return null;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(native);
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
        picker.FileTypeFilter.Add(".json");

        var file = await picker.PickSingleFileAsync();
        if (file is null)
            return null;

        return await File.ReadAllTextAsync(file.Path);
    }
}
