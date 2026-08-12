using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.AuditLogs;
using Shabakat.Application.Helper;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Services.AuditLogs;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IAuditLogRepository auditLogRepository,
        ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task LogSuccessAsync(AuditLogWriteRequest entry)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = entry.Action,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Summary = entry.Summary,
                Details = entry.Details,
                Status = AuditLogStatus.Success,
                CreatedAt = DateTime.Now
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();

            _logger.LogDebug(
                "Audit recorded: {Action} — {Summary}",
                entry.Action,
                entry.Summary);
        }
        catch (Exception ex)
        {
            // Audit failures must not break the business operation.
            _logger.LogWarning(
                ex,
                "Failed to write audit log for {Action}: {Summary}",
                entry.Action,
                entry.Summary);
        }
    }

    public async Task<PagedResponse<AuditLogResponse>> GetAllAsync(AuditLogFilterRequest filter)
    {
        var (items, totalCount) = await _auditLogRepository.GetAllPagedAsync(filter);

        return PagedResponse<AuditLogResponse>.Create(
            data: items.Select(Map),
            totalCount: totalCount,
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize);
    }

    private static AuditLogResponse Map(AuditLog log) =>
        new(
            Id: log.Id,
            Action: log.Action.ToString(),
            Status: log.Status.ToString(),
            Summary: log.Summary,
            Details: log.Details,
            EntityType: log.EntityType,
            EntityId: log.EntityId,
            ErrorMessage: log.ErrorMessage,
            CreatedAt: log.CreatedAt);
}
