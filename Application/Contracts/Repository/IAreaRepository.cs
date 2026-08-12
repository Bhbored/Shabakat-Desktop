using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IAreaRepository : IGenericRepository<Area>
{
    Task<IEnumerable<Area>> GetAllWithCustomerCountAsync();
    Task<bool> HasCustomersAsync(Guid areaId);
}
