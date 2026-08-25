namespace Shabakat.Application.Options;

public sealed class CloudBackupOptions
{
    public const string SectionName = "CloudBackup";

    public string WorkerUrl { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}
