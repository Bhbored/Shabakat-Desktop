using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class AppPreferencesRepository : GenericRepository<AppPreferences>, IAppPreferencesRepository
{
    public AppPreferencesRepository(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

    public async Task<AppPreferences?> GetAsync()
        => await _dbSet
            .Include(p => p.CustomerExportColumnPreference)
            .FirstOrDefaultAsync();
}
