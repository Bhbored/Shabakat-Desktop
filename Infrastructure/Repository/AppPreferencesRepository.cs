using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class AppPreferencesRepository : GenericRepository<AppPreferences>, IAppPreferencesRepository
{
    public AppPreferencesRepository(AppDbContext db) : base(db) { }

    public async Task<AppPreferences?> GetAsync()
        => await _dbSet.FirstOrDefaultAsync();
}
