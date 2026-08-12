using Shabakat.Domain.Entities;

namespace Shabakat.Application.Contracts.Repository;

public interface IMeterReadingRepository : IGenericRepository<MeterReading>
{
    Task<MeterReading?> GetLatestBeforeAsync(
        Guid customerId, DateOnly beforeDate, Guid? excludeReadingId = null);
    Task<IEnumerable<MeterReading>> GetAllForCustomerAsync(Guid customerId);
    Task<IEnumerable<MeterReading>> GetAllWithCustomerAsync();
    Task<MeterReading?> GetLatestForCustomerAsync(Guid customerId);
    Task<MeterReading?> GetForPeriodAsync(Guid customerId, DateOnly periodStart, DateOnly periodEnd);
    Task<MeterReading?> GetInitialForCustomerAsync(Guid customerId);
    Task<bool> HasOfficialReadingAsync(Guid customerId);
}
