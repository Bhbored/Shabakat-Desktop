using System.ComponentModel.DataAnnotations;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Expenses;

public record ExpenseResponse(
    Guid Id,
    string ExpenseType,
    decimal Amount,
    DateOnly ExpenseDate,
    string? Label,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateExpenseRequest(
    [Required] ExpenseType ExpenseType,
    [Required][Range(0.0001, double.MaxValue)] decimal Amount,
    DateOnly? ExpenseDate = null,
    [MaxLength(100)] string? Label = null,
    [MaxLength(500)] string? Notes = null);

public record UpdateExpenseRequest(
    ExpenseType? ExpenseType = null,
    [Range(0.0001, double.MaxValue)] decimal? Amount = null,
    DateOnly? ExpenseDate = null,
    [MaxLength(100)] string? Label = null,
    [MaxLength(500)] string? Notes = null);

public record ExpenseListResponse(
    IEnumerable<ExpenseResponse> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    decimal TotalAmount);
