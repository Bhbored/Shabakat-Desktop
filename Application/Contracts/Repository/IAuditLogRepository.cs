using Shabakat.Application.DTOs.AuditLogs;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllPagedAsync(
        AuditLogFilterRequest filter,
        CancellationToken cancellationToken = default);
}
