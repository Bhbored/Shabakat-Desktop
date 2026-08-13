using Shabakat.Domain.Enums;

namespace Shabakat.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public AuditAction Action { get; set; }
    public AuditEntityType? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Details { get; set; }
    public AuditLogStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
