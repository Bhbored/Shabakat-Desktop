using Shabakat.Application.DTOs.Expenses;

namespace Shabakat.Application.Contracts.Services;

public interface IExpenseService
{
    Task<ExpenseListResponse> GetAllAsync(ExpenseFilterRequest filter);
    Task<IEnumerable<ExpenseResponse>> GetAllUnpagedAsync();
    Task<ExpenseResponse> GetByIdAsync(Guid id);
    Task<ExpenseResponse> CreateAsync(CreateExpenseRequest request);
    Task<ExpenseResponse> UpdateAsync(Guid id, UpdateExpenseRequest request);
    Task DeleteAsync(Guid id);
}
