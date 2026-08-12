using Shabakat.Application.DTOs.Expenses;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IExpenseRepository : IGenericRepository<Expenses>
{
    Task<(IEnumerable<Expenses> Items, int TotalCount)> GetAllPagedAsync(ExpenseFilterRequest filter);
    Task<IEnumerable<Expenses>> GetAllMatchingAsync(ExpenseFilterRequest filter);
}
