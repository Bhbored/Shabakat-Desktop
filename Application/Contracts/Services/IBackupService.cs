using Shabakat.Application.Backup;

namespace Shabakat.Application.Contracts.Services;

public interface IBackupService
{
    IAsyncEnumerable<double> ExportAsync(string destinationPath, CancellationToken cancellationToken = default);
    IAsyncEnumerable<double> RestoreAsync(string json, CancellationToken cancellationToken = default);
}
