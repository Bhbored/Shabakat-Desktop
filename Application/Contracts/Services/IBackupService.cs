namespace Shabakat.Application.Contracts.Services;

public interface IBackupService
{
    Task<string> ExportAsync();
    Task RestoreAsync(string json);
}
