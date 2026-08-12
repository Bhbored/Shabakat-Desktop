using Shabakat.Application.DTOs.DistributionBox;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IDistributionBoxRepository : IGenericRepository<DistributionBox>
{
    Task<(IEnumerable<DistributionBox> Items, int TotalCount)> GetAllPagedAsync(DistributionBoxFilterRequest filter);
    Task<IEnumerable<DistributionBox>> GetAllWithDetailsAsync();
    Task<DistributionBox?> GetByIdWithDetailsAsync(Guid id);
    Task<bool> HasCustomersAsync(Guid boxId);
}
