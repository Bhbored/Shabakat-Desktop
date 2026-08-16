using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.Helper;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Application.Services.License;

public sealed class LicenseService : ILicenseService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher;
    private readonly ILogger<LicenseService> _logger;

    public event Action? Changed;

    public LicenseService(
        AppDbContext db,
        PasswordHasher<AppUser> hasher,
        ILogger<LicenseService> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<LicenseStatus> GetStatusAsync()
    {
        var user = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync();
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
            return LicenseStatus.SetupRequired;

        return DateTimeOffset.UtcNow < user.LicensedUntil
            ? LicenseStatus.Active
            : LicenseStatus.Expired;
    }

    public async Task SetupAsync(string pin, DateOnly expiryDate, TimeOnly expiryTime)
    {
        EnsurePin(pin);
        var licensedUntil = ToFutureUtc(expiryDate, expiryTime);

        var user = await _db.AppUsers.FirstOrDefaultAsync();
        if (user is not null && !string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new DomainException("The app is already activated.");

        if (user is null)
        {
            user = new AppUser();
            await _db.AppUsers.AddAsync(user);
        }

        user.PasswordHash = _hasher.HashPassword(user, pin.Trim());
        user.LicensedUntil = licensedUntil;
        await _db.SaveChangesAsync();
        _logger.LogInformation("License set up until {LicensedUntil}", licensedUntil);
        Changed?.Invoke();
    }

    public async Task RenewAsync(string pin, DateOnly expiryDate, TimeOnly expiryTime)
    {
        EnsurePin(pin);
        var licensedUntil = ToFutureUtc(expiryDate, expiryTime);

        var user = await _db.AppUsers.FirstOrDefaultAsync();
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new DomainException("The app is not activated yet.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, pin.Trim());
        if (result == PasswordVerificationResult.Failed)
            throw new DomainException("Invalid activation key.");

        user.LicensedUntil = licensedUntil;
        await _db.SaveChangesAsync();
        _logger.LogInformation("License renewed until {LicensedUntil}", licensedUntil);
        Changed?.Invoke();
    }

    public void NotifyChanged() => Changed?.Invoke();

    private static void EnsurePin(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Trim().Length < 4)
            throw new DomainException("Activation key must be at least 4 characters.");
    }

    private static DateTimeOffset ToFutureUtc(DateOnly expiryDate, TimeOnly expiryTime)
    {
        var until = BeirutTime.ToUtc(expiryDate, expiryTime);
        if (until <= DateTimeOffset.UtcNow)
            throw new DomainException("Expiry must be in the future (Beirut time).");
        return until;
    }
}
