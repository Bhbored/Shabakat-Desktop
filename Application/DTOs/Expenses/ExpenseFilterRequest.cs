using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Expenses;

public record ExpenseFilterRequest(
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    ExpenseType? ExpenseType = null,
    int PageNumber = 1,
    int PageSize = 10);
