using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.AuditLogs;

public record AuditLogDetailItem(string Label, string Value);

public record AuditLogResponse(
    Guid Id,
    string Action,
    string Status,
    string Summary,
    IReadOnlyList<AuditLogDetailItem> Details,
    string? EntityType,
    Guid? EntityId,
    string? ErrorMessage,
    DateTime CreatedAt);

public record AuditLogWriteRequest(
    AuditAction Action,
    string Summary,
    AuditEntityType? EntityType = null,
    Guid? EntityId = null,
    IReadOnlyList<AuditLogDetailItem>? Details = null);
