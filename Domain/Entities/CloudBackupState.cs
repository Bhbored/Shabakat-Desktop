using Shabakat.Domain.Common;

namespace Shabakat.Domain.Entities;

public class CloudBackupState : Base
{
    public Guid InstallId { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime? LastSuccessfulUploadAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string? LastObjectKey { get; set; }
    public string? LastError { get; set; }
}
