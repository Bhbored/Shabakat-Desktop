using Shabakat.Application.Backup;

namespace Shabakat.Application.Contracts.Repository;

public interface IBackupRepository
{
    IAsyncEnumerable<double> LoadAsync(BackupFile destination, CancellationToken cancellationToken = default);
    IAsyncEnumerable<double> ReplaceAsync(BackupFile file, CancellationToken cancellationToken = default);
}
