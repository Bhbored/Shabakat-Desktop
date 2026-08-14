using Shabakat.Application.DTOs.Expenses;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Mappers;

public static class ExpenseMapper
{
    public static ExpenseResponse ToResponse(this Expenses e) =>
        new(
            Id: e.Id,
            ExpenseType: e.ExpenseType.ToString(),
            Amount: e.Amount,
            ExpenseDate: e.ExpenseDate,
            Label: e.Label,
            Notes: e.Notes,
            CreatedAt: e.CreatedAt,
            UpdatedAt: e.UpdatedAt);
}
