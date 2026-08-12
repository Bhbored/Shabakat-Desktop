using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Domain.Common;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : Base
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger _logger;

    public GenericRepository(AppDbContext db, ILoggerFactory loggerFactory)
    {
        _db = db;
        _dbSet = db.Set<T>();
        _logger = loggerFactory.CreateLogger($"Shabakat.Repository.{typeof(T).Name}");
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
    {
        var count = await _db.SaveChangesAsync();
        if (count > 0)
            _logger.LogDebug("Saved {ChangeCount} change(s)", count);
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await action();
            await transaction.CommitAsync();
            _logger.LogDebug("Transaction committed");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Transaction rolled back");
            throw;
        }
    }
}
