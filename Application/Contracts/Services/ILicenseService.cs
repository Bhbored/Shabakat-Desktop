using Shabakat.Domain.Enums;

namespace Shabakat.Application.Contracts.Services;

public interface ILicenseService
{
    event Action? Changed;

    Task<LicenseStatus> GetStatusAsync();
    Task SetupAsync(string pin, DateOnly expiryDate, TimeOnly expiryTime);
    Task RenewAsync(string pin, DateOnly expiryDate, TimeOnly expiryTime);
    void NotifyChanged();
}
