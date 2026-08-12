using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IAppPreferencesRepository : IGenericRepository<AppPreferences>
{
    Task<AppPreferences?> GetAsync();
}
