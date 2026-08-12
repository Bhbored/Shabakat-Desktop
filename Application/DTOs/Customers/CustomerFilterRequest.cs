using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Customers;

public record CustomerFilterRequest(
    string? Name = null,
    string? Phone = null,
    Guid? AreaId = null,
    Guid? BoxId = null,
    Guid? AmpereScheduleId = null,
    PlanType? PlanType = null,
    CustomerRelation? CustomerRelation = null,
    CustomerStatus? CustomerStatus = null,
    string? PaymentFilter = null,
    int PageNumber = 1,
    int PageSize = 10);
