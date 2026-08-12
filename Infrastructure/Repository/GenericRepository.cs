using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Domain.Common;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : Base
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext db)
    {
        _db = db;
        _dbSet = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
        => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Delete(T entity)
        => _dbSet.Remove(entity);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await action();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
