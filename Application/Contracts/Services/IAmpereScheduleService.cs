using Shabakat.Application.DTOs.AmpereSchedule;

namespace Shabakat.Application.Contracts.Services;

public interface IAmpereScheduleService
{
    Task<IEnumerable<AmpereScheduleResponse>> GetAllAsync();
    Task<AmpereScheduleResponse> CreateAsync(CreateAmpereScheduleRequest request);
    Task<AmpereScheduleResponse> UpdateAsync(Guid id, UpdateAmpereScheduleRequest request);
    Task DeleteAsync(Guid id);
}
