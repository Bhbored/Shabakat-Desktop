using Shabakat.Application.DTOs.AuditLogs;
using Shabakat.Application.Helper;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Contracts.Services;

public interface IAuditLogService
{
    Task LogSuccessAsync(AuditLogWriteRequest entry);
    Task<PagedResponse<AuditLogResponse>> GetAllAsync(AuditLogFilterRequest filter);
}
