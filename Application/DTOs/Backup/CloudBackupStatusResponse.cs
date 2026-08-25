namespace Shabakat.Application.DTOs.Backup;

public sealed record CloudBackupStatusResponse(
    bool Configured,
    bool Enabled,
    DateTime? LastSuccessfulUploadAt,
    DateTime? LastAttemptAt,
    string? LastObjectKey,
    string? LastError);
