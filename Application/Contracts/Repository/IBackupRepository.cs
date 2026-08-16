using Shabakat.Application.Backup;

namespace Shabakat.Application.Contracts.Repository;

public interface IBackupRepository
{
    Task<BackupFile> LoadAsync();
    Task ReplaceAsync(BackupFile file);
}
