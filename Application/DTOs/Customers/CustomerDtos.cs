using System.ComponentModel.DataAnnotations;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Customers;

public record CustomerResponse(
    Guid Id,
    string Name,
    string? Phone,
    string? Address,
    string? Building,
    string? Floor,
    string? CableName,
    Guid? BoxId,
    string? BoxName,
    Guid? AmpereScheduleId,
    string? AmpereScheduleName,
    string CustomerType,
    string Plan,
    decimal PlanValue,
    decimal? InitialMeterReading,
    string? AreaName,
    string CustomerStatus,
    DateOnly SubscriptionDate,
    DateTime CreatedAt,
    string? CustomerRelation,
    bool HasPricingOverride,
    CustomerPricingOverrideDto? PricingOverride,
    decimal TotalBilled,
    decimal TotalPaid,
    decimal TotalOutstanding,
    bool PaidThisMonth);

public record CustomerSummaryResponse(
    Guid Id,
    string Name,
    string? Phone,
    string? Address,
    string? Building,
    string? Floor,
    string? CableName,
    Guid? BoxId,
    string? BoxName,
    Guid? AmpereScheduleId,
    string? AmpereScheduleName,
    string CustomerType,
    string Plan,
    string? AreaName,
    decimal PlanValue,
    string CustomerStatus,
    DateOnly SubscriptionDate,
    DateTime CreatedAt,
    bool HasPricingOverride,
    string? CustomerRelation,
    decimal AmountDue);

public record CreateCustomerRequest(
    [Required][MaxLength(200)] string Name,
    [Phone][MaxLength(30)] string? Phone = null,
    [MaxLength(500)] string? Address = null,
    [MaxLength(100)] string? Building = null,
    [MaxLength(50)] string? Floor = null,
    [MaxLength(100)] string? CableName = null,
    Guid? BoxId = null,
    Guid? AmpereScheduleId = null,
    Guid? AreaId = null,
    [Required] CustomerType CustomerType = default,
    [Required] PlanType Plan = default,
    [Required][Range(0.01, 9999999)] decimal PlanValue = 0,
    DateOnly? SubscriptionDate = null,
    CustomerRelation? CustomerRelation = null,
    CustomerPricingOverrideDto? PricingOverride = null,
    [Range(0, 9999999)] decimal? InitialMeterReading = null);

public record UpdateCustomerRequest(
    [MaxLength(200)] string? Name = null,
    [Phone][MaxLength(30)] string? Phone = null,
    Guid? AreaId = null,
    [MaxLength(500)] string? Address = null,
    [MaxLength(100)] string? Building = null,
    [MaxLength(50)] string? Floor = null,
    [MaxLength(100)] string? CableName = null,
    Guid? BoxId = null,
    Guid? AmpereScheduleId = null,
    CustomerType? CustomerType = null,
    PlanType? Plan = null,
    [Range(0.01, 9999999)] decimal? PlanValue = null,
    CustomerStatus? CustomerStatus = null,
    CustomerRelation? CustomerRelation = null,
    CustomerPricingOverrideDto? PricingOverride = null,
    bool ClearPricingOverride = false,
    [Range(0, 9999999)] decimal? InitialMeterReading = null);

public record CustomerPricingOverrideDto(
    decimal? Price,
    decimal? FixedCharge,
    decimal? TVA);

public record SuspendCustomersRequest(
    [Required][MinLength(1)] IReadOnlyList<Guid> CustomerIds);

public record SuspendCustomersResponse(
    int Suspended,
    string Message);
