using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IAmpereScheduleRepository : IGenericRepository<AmpereSchedule>
{
    Task<IEnumerable<AmpereSchedule>> GetAllWithCustomerCountAsync();
    Task<AmpereSchedule?> GetByIdWithDetailsAsync(Guid id);
    Task<bool> HasCustomersAsync(Guid scheduleId);
    Task<bool> HasAnyAsync();
    Task<bool> HoursPerDayExistsAsync(int hoursPerDay, Guid? excludeId = null);
}
