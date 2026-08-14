using Shabakat.Application.DTOs.AuditLogs;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Mappers;

public static class AuditLogMapper
{
    public static AuditLogResponse ToResponse(this AuditLog log) =>
        new(
            Id: log.Id,
            Action: log.Action.ToString(),
            Status: log.Status.ToString(),
            Summary: log.Summary,
            Details: log.Details
                .Select(d => new AuditLogDetailItem(d.Label, d.Value))
                .ToList(),
            EntityType: log.EntityType?.ToString(),
            EntityId: log.EntityId,
            ErrorMessage: log.ErrorMessage,
            CreatedAt: log.CreatedAt);
}
