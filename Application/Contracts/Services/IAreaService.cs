using Shabakat.Application.DTOs.Area;

namespace Shabakat.Application.Contracts.Services;

public interface IAreaService
{
    Task<IEnumerable<AreaResponse>> GetAllAsync();
    Task<AreaResponse> GetByIdAsync(Guid id);
    Task<AreaResponse> CreateAsync(CreateAreaRequest request);
    Task<AreaResponse> UpdateAsync(Guid id, UpdateAreaRequest request);
    Task DeleteAsync(Guid id);
}
