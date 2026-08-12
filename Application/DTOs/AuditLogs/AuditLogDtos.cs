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

public static class AuditEntityTypes
{
    public const string Customer = "Customer";
    public const string Invoice = "Invoice";
    public const string Payment = "Payment";
    public const string Expense = "Expense";
}

public record AuditLogWriteRequest(
    AuditAction Action,
    string Summary,
    string? EntityType = null,
    Guid? EntityId = null,
    string? Details = null);
