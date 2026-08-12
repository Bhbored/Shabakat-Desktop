using Shabakat.Application.DTOs.AuditLogs;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task SaveChangesAsync();
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllPagedAsync(AuditLogFilterRequest filter);
}
