using Shabakat.Application.DTOs.Exports;

namespace Shabakat.Application.Helper;

public static class ExcelFileSaver
{
    public static async Task<bool> SaveCustomerExportAsync(CustomerExportFile file)
        => await SaveWorkbookAsync(file.Content, file.FileName);

    public static async Task<bool> SaveUnpaidInvoiceExportAsync(UnpaidInvoiceExportFile file)
        => await SaveWorkbookAsync(file.Content, file.FileName);

    private static async Task<bool> SaveWorkbookAsync(byte[] content, string fileName)
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        var native = window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (native is null)
            return false;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(native);
        var picker = new Windows.Storage.Pickers.FileSavePicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
        picker.SuggestedFileName = Path.GetFileNameWithoutExtension(fileName);
        picker.FileTypeChoices.Add("Excel workbook", [".xlsx"]);

        var dest = await picker.PickSaveFileAsync();
        if (dest is null)
            return false;

        await File.WriteAllBytesAsync(dest.Path, content);
        return true;
    }
}
