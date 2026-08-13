using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.AuditLogs;

public record AuditLogResponse(
    Guid Id,
    string Action,
    string Status,
    string Summary,
    string? Details,
    string? EntityType,
    Guid? EntityId,
    string? ErrorMessage,
    DateTime CreatedAt);

public record AuditLogWriteRequest(
    AuditAction Action,
    string Summary,
    AuditEntityType? EntityType = null,
    Guid? EntityId = null,
    string? Details = null);
