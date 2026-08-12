using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.AuditLogs;

public record AuditLogFilterRequest(
    AuditAction? Action = null,
    AuditLogStatus? Status = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    int PageNumber = 1,
    int PageSize = 20);
