using Shabakat.Application.DTOs.Exports;

namespace Shabakat.Application.Helper;

public static class ExcelFileSaver
{
    public static async Task<bool> SaveAsync(CustomerExportFile file)
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        var native = window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (native is null)
            return false;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(native);
        var picker = new Windows.Storage.Pickers.FileSavePicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
        picker.SuggestedFileName = Path.GetFileNameWithoutExtension(file.FileName);
        picker.FileTypeChoices.Add("Excel workbook", [".xlsx"]);

        var dest = await picker.PickSaveFileAsync();
        if (dest is null)
            return false;

        await File.WriteAllBytesAsync(dest.Path, file.Content);
        return true;
    }
}
