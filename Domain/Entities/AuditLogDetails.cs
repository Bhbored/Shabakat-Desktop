namespace Shabakat.Domain.Entities;

public class AuditLogDetails
{
    public Guid Id { get; set; }
    public Guid AuditLogId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public AuditLog AuditLog { get; set; } = null!;
}
