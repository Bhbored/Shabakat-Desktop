using Shabakat.Domain.Common;

namespace Shabakat.Application.Contracts.Repository;

public interface IGenericRepository<T> where T : Base
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
    Task ExecuteInTransactionAsync(Func<Task> action);
}
