using Shabakat.Application.DTOs.DistributionBox;
using Shabakat.Application.Helper;

namespace Shabakat.Application.Contracts.Services;

public interface IDistributionBoxService
{
    Task<PagedResponse<DistributionBoxResponse>> GetAllAsync(DistributionBoxFilterRequest filter);
    Task<IEnumerable<DistributionBoxResponse>> GetAllUnpagedAsync();
    Task<DistributionBoxResponse> CreateAsync(CreateDistributionBoxRequest request);
    Task<DistributionBoxResponse> UpdateAsync(Guid id, UpdateDistributionBoxRequest request);
    Task DeleteAsync(Guid id);
}
