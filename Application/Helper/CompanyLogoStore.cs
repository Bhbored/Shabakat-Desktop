using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Helper;

public static class CompanyLogoStore
{
    private static readonly HashSet<string> AllowedExtensions =
    [
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp", ".svg"
    ];

    public static string FolderPath =>
        Path.Combine(FileSystem.AppDataDirectory, "logo");

    public static async Task<string?> PickAsync()
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        var native = window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (native is null)
            return null;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(native);
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".gif");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".svg");

        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }

    public static string Replace(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            throw new DomainException("Error.LogoFileNotFound");

        var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new DomainException("Error.LogoFileType");

        var folder = FolderPath;
        Directory.CreateDirectory(folder);

        var destination = Path.Combine(folder, "logo" + extension);
        File.Copy(sourcePath, destination, overwrite: true);

        foreach (var existing in Directory.EnumerateFiles(folder))
        {
            if (!string.Equals(existing, destination, StringComparison.OrdinalIgnoreCase))
                File.Delete(existing);
        }

        return destination;
    }

    public static string? ToDataUri(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        var mime = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };

        var bytes = File.ReadAllBytes(path);
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }
}
