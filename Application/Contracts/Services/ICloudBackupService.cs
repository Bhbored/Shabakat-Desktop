using Shabakat.Application.DTOs.Backup;

namespace Shabakat.Application.Contracts.Services;

public interface ICloudBackupService
{
    Task<CloudBackupStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);
    Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default);
    Task UploadNowAsync(CancellationToken cancellationToken = default);
    Task TryScheduledUploadAsync(CancellationToken cancellationToken = default);
}
