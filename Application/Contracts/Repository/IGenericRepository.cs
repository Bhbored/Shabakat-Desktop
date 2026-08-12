using Shabakat.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shabakat.Application.Contracts.Repository
{
    public interface IGenericRepository<T> where T : Base
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        void Update(T entity);
        void SoftDelete(T entity);
        Task SaveChangesAsync();
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}
