using Shabakat.Application.DTOs.Customers;
using Shabakat.Application.Helper;

namespace Shabakat.Application.Contracts.Services;

public interface ICustomerService
{
    Task<PagedResponse<CustomerSummaryResponse>> GetAllAsync(CustomerFilterRequest filter);
    Task<IEnumerable<CustomerResponse>> GetAllUnpagedAsync();
    Task<CustomerResponse> GetByIdAsync(Guid id);
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request);
    Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request);
    Task DeleteAsync(Guid id);
    Task<SuspendCustomersResponse> SuspendAsync(SuspendCustomersRequest request);
    Task<TerminateCustomersResponse> TerminateAsync(TerminateCustomersRequest request);
}
