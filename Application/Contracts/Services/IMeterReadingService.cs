using Shabakat.Application.DTOs.MeterReadings;

namespace Shabakat.Application.Contracts.Services;

public interface IMeterReadingService
{
    Task<IEnumerable<MeterReadingResponse>> GetAllForCustomerAsync(Guid customerId);
    Task<IEnumerable<MeterReadingListItemResponse>> GetAllUnpagedAsync();
    Task<MeterReadingResponse> GetLatestForCustomerAsync(Guid customerId);
    Task<MeterReadingResponse> CreateAsync(Guid customerId, CreateMeterReadingRequest request);
    Task DeleteAsync(Guid customerId, Guid readingId);
}
