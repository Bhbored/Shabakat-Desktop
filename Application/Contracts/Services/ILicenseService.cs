using Shabakat.Domain.Enums;

namespace Shabakat.Application.Contracts.Services;

public sealed record LicenseRemaining(int DaysLeft, DateOnly RenewOn);

public interface ILicenseService
{
    event Action? Changed;

    Task<LicenseStatus> GetStatusAsync();
    Task<LicenseRemaining?> GetRemainingAsync();
    Task SetupAsync(string pin, DateOnly expiryDate, TimeOnly expiryTime);
    Task RenewAsync(string pin, DateOnly expiryDate, TimeOnly expiryTime);
    Task NotifyRestoredAsync();
    LicenseRestoreNotice TakeRestoreNotice();
    void NotifyChanged();
}
